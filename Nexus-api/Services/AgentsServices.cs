
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.ML.OnnxRuntimeGenAI;
using Microsoft.Extensions.Options;
using Nexus_api.Configuration;
using Nexus_api.Infrastructure.Pinecone;
using Pinecone;

namespace Nexus_api.Services;

public class AgentsServices
{
    private readonly Kernel _kernel;
    private readonly IKernelMemory _kernelMemory;
    private readonly IMemoryCache _memoryCache;
    private readonly AppSettings _appSettings;
    private readonly PineconeVectorDbRepository _pineconeRepository;

    public AgentsServices(
        Kernel kernel, 
        IKernelMemory kernelMemory, 
        IMemoryCache memoryCache, 
        IOptions<AppSettings> options,
        PineconeVectorDbRepository pineconeRepository)
    {
        _kernel = kernel;
        _kernelMemory = kernelMemory;
        _memoryCache = memoryCache;
        _appSettings = options.Value;
        _pineconeRepository = pineconeRepository;
    }

    private const string EmbeddingModel = "text-embedding-3-small";

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
            if (_memoryCache.TryGetValue(cacheKey, out List<(string, string)>? history))
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
            _memoryCache.Set(cacheKey, history, cacheOptions);
        }



        public async Task<string> Chat(string prompt, string userId = "default")
        {
            // 📝 Obtener historial de conversación
            var conversationHistory = GetConversationHistory(userId);
            var responseText = string.Empty;
            // Crear ChatHistory para conversación estructurada
            var chatHistory = new ChatHistory();
            foreach (var (role, message) in conversationHistory)
            {
                var authorRole = role == "usuario" ? AuthorRole.User : AuthorRole.Assistant;
                chatHistory.Add(new ChatMessageContent(authorRole, message));
            }
            
            // Agregar el prompt del usuario actual
            chatHistory.AddUserMessage(prompt);
            
            //🔍 Buscar en Kernel Memory y Pinecone
            var searchResultTool = await _kernelMemory.SearchAsync(
                query: prompt,
                limit: 3,
                minRelevance: 0.5f
            );
            var embeddingPrompt = await Infrastructure.Utils.CreateOpenAiEmbeddingAsync(
                _appSettings.Nexus.ApiKey, EmbeddingModel, prompt);

            QueryResponse searchResult = await _pineconeRepository.QueryAsync(embeddingPrompt, topK: 3);
            
            var texts = searchResultTool.NoResult ? 
                (searchResult?.Matches != null ? searchResult.Matches.Select(m => m.Metadata["ChunkText"]?.ToString() ?? string.Empty) : Enumerable.Empty<string>())
                : (searchResultTool.Results != null ? searchResultTool.Results.Select(r => r.Partitions[0].Text) : Enumerable.Empty<string>());
            
            var retrievedContent = string.Join("\n\n", texts.Where(t => !string.IsNullOrEmpty(t)));

            // Si hay contenido relevante, agregarlo como mensaje de sistema
            if (!string.IsNullOrEmpty(retrievedContent))
            {
                //chatHistory.AddSystemMessage($"Información de la base de conocimiento:\n{retrievedContent}");
            }
            // Si no hay contenido, dejar que la IA decida invocar herramientas
            
            // Invocar con herramientas habilitadas
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            var chatMessageContents = await chatCompletionService.GetChatMessageContentsAsync(chatHistory, settings, _kernel);
            responseText = chatMessageContents.FirstOrDefault()?.Content ?? string.Empty;


            // 💾 Guardar el intercambio en el historial
            conversationHistory.Add(("usuario", prompt));
            conversationHistory.Add(("asistente", responseText));
            SaveConversationHistory(userId, conversationHistory);

            return responseText;
        }
}