using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Caching.Memory;
using Qdrant.Client;
using Microsoft.SemanticKernel.Embeddings;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using Nexus_api.Dtos;

namespace Nexus_api.Services;

public class AgentsServices(
    Kernel kernel, 
    IMemoryCache memoryCache,
    QdrantClient qdrantClient,
    ITextEmbeddingGenerationService embeddingService)
{

   /// <summary>
        /// En Semantic Kernel (SK), ese código configura cómo el kernel debe decidir 
        /// automáticamente si ejecuta o no una Function (Skill) de tu aplicación cuando 
        /// el modelo de IA lo considere necesario.
        /// </summary>
        /// <returns></returns>
        private static readonly OpenAIPromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ChatSystemPrompt = """
             Instrucciones obligatorias: responde siempre en español; 
             si el usuario saluda, comienza con un saludo cordial; 
             busca primero en tu memoria o base de conocimiento y, 
             si hay una respuesta completa y verificada, 
             úsala tal cual; si no existe, usa los plugins adecuados 
             y si estos devuelven una respuesta completa, 
             preséntala sin cambios; 
             si ni memoria ni plugins contienen la respuesta, 
             no generes una por tu cuenta; 
             sigue siempre el orden Memoria->Plugins y nunca combines fuentes.
             Instrucciones obligatorias: responde siempre en español; 
             si el usuario saluda, comienza con un saludo cordial; 
             busca primero en tu memoria o base de conocimiento y, 
             si hay una respuesta completa y verificada, 
             úsala tal cual; si no existe, usa los plugins adecuados 
             y si estos devuelven una respuesta completa, 
             preséntala sin cambios; 
             si ni memoria ni plugins contienen la respuesta, 
             no generes una por tu cuenta; 
             sigue siempre el orden Memoria->Plugins y nunca combines fuentes.
            """
        };
        /// <summary>
        /// Cada vez que ejecutes una función o prompt en el Kernel y uses arguments, Semantic Kernel 
        /// aplicará automáticamente la configuración OpenAIPromptExecutionSettings que definiste.
        /// </summary>
        private static readonly KernelArguments arguments = new (settings);
        
        private const string HistoryCacheKeyPrefix = "conversation_history_";
        private const int HistoryCacheDurationMinutes = 30;

        /// <summary>
        /// Obtiene el historial de conversación para un usuario específico
        /// </summary>
        private List<(string role, string message)> GetConversationHistory(string userId)
        {
            var cacheKey = $"{HistoryCacheKeyPrefix}{userId}";
            if (memoryCache.TryGetValue(cacheKey, out List<(string, string)>? history))
            {
                return history ?? [];
            }
            return [];
        }

        /// <summary>
        /// Guarda el historial de conversación en caché
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="history"></param>
        private void SaveConversationHistory(string userId, List<(string role, string message)> history)
        {
            var cacheKey = $"{HistoryCacheKeyPrefix}{userId}";
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(HistoryCacheDurationMinutes));
            memoryCache.Set(cacheKey, history, cacheOptions);
        }

        /// <summary>
        /// Formatea el historial de conversación como texto
        /// </summary>
        private static string FormatConversationHistory(List<(string role, string message)> history)
        {
            if (history.Count == 0)
                return string.Empty;

            var historyText = "Historial de conversación:\n";
            foreach (var (role, message) in history.TakeLast(10)) // Últimos 10 mensajes
            {
                historyText += $"{role}: {message}\n";
            }
            return historyText;
        }

        public async Task<string> Chat(string prompt, string userId = "default")
        {
            // 📝 Obtener historial de conversación
            var conversationHistory = GetConversationHistory(userId);
            // 🔍 1. Buscar en Kernel Memory (Qdrant)
        
            var searchResult = await kernel.InvokePromptAsync(
                $"""
                Busca en tu memoria semántica información relevante para responder a la siguiente pregunta del usuario: {prompt}
                Devuelve solo el texto encontrado sin agregar nada más.
                Regresa en estructura JSON: FoundInSemanticMemory (booleano) y ResponseContent (string con el texto encontrado o vacío si no se encontró nada).
                """, 
                arguments);
            string jsonString = searchResult.GetValue<string>() ?? string.Empty;
            var dto = JsonSerializer.Deserialize<ChatResponseDto>(jsonString);
          
            if (dto!.FoundInSemanticMemory)
            {
                // 💾 Guardar el intercambio en el historial
                conversationHistory.Add(("usuario", prompt));
                conversationHistory.Add(("asistente", dto.ResponseContent));
                SaveConversationHistory(userId, conversationHistory);
                return dto.ResponseContent;
            }
            var vector = await embeddingService.GenerateEmbeddingsAsync([prompt], null, CancellationToken.None);
            IReadOnlyList<Qdrant.Client.Grpc.ScoredPoint>? buscaquedaQdrant = await qdrantClient.SearchAsync(
                collectionName: "pdfs",
                vector: vector[0].ToArray(),
                limit: 1
            );
            string historyContext = FormatConversationHistory(conversationHistory);
            string fullPrompt = $"""
                Hitorial de la conversación:
                {historyContext}

                Información de la base de conocimiento:
                {buscaquedaQdrant[0].Payload["text"]}

                Pregunta del usuario: {prompt}
                """;

            var result = await kernel.InvokePromptAsync(fullPrompt, arguments);
            var responseText = result.GetValue<string>() ?? string.Empty;

            // 💾 Guardar el intercambio en el historial
            conversationHistory.Add(("usuario", prompt));
            conversationHistory.Add(("asistente", responseText));
            SaveConversationHistory(userId, conversationHistory);

            return responseText;
        }

        
}

