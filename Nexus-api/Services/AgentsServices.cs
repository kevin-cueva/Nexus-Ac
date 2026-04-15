using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Caching.Memory;
using Qdrant.Client;
using Microsoft.SemanticKernel.Embeddings;
using System.Text.Json;
using Nexus_api.Dtos;
using Nexus_api.Services.Interface;
using Nexus_api.Infrastructure.Utils;

namespace Nexus_api.Services;

public class AgentsServices(
    Kernel kernel,
    IMemoryCache memoryCache,
    QdrantClient qdrantClient,
    IClienServices clienServices,
    ITextEmbeddingGenerationService embeddingService)
{
    private bool _isInitialized = false;
    private static readonly KernelArguments arguments = Constants.ExecutionSettings.CreateKernelArguments();

    public async Task<string> Chat(string prompt, string userId = "default")
    {
        var conversationHistory = GetConversationHistory(userId);
        if (!_isInitialized && conversationHistory.Count == 0)
        {
           var allcases = await clienServices.SizeCasesBySector();
           var jsonCases = JsonSerializer.Serialize(allcases);
           conversationHistory.Add(("sistema", $"Información de cantidad de casos por sector: {jsonCases}"));
           _isInitialized = true;
        }

        string semanticContext = await SearchSemanticMemory(prompt);

        string qdrantContext = await FormatQdrantResults(await SearchInQdrantAsync(prompt));

        string fusedContext = FuseContexts(semanticContext, qdrantContext);

        string historyContext = FormatConversationHistory(conversationHistory);

        string fullPrompt = $"""
            Historial de la conversación:
            {historyContext}

            Contexto:
            {fusedContext}

            Pregunta del usuario: {prompt}
            """;

        var result = await kernel.InvokePromptAsync(fullPrompt, arguments);
        var responseText = result.GetValue<string>() ?? string.Empty;

        conversationHistory.Add(("usuario", prompt));
        conversationHistory.Add(("asistente", responseText));
        SaveConversationHistory(userId, conversationHistory);

        return responseText;
    }

    /// <summary>
    /// Guarda el historial de conversación en caché
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="history"></param>
    private void SaveConversationHistory(string userId, List<(string role, string message)> history)
    {
        var cacheKey = $"{Constants.Llm.MemoryCacheKeyPrefix}{userId}";
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(Constants.Llm.HistoryCacheDurationMinutes));
        memoryCache.Set(cacheKey, history, cacheOptions);
    }

    /// <summary>
    /// Obtiene el historial de conversación para un usuario específico
    /// </summary>
    private List<(string role, string message)> GetConversationHistory(string userId)
    {
        var cacheKey = $"{Constants.Llm.MemoryCacheKeyPrefix}{userId}";
        if (memoryCache.TryGetValue(cacheKey, out List<(string, string)>? history))
        {
            return history ?? [];
        }
        return [];
    }


    /// <summary>
    /// Formatea el historial de conversación como texto
    /// </summary>
    private static string FormatConversationHistory(List<(string role, string message)> history)
    {
        if (history.Count == 0)
            return Constants.Formatting.EmptyHistory;

        var historyText = $"{Constants.Formatting.HistoryHeader}\n";
        foreach (var (role, message) in history.TakeLast(Constants.Llm.MaxHistoryMessages))
        {
            historyText += string.Format(Constants.Formatting.HistoryEntryFormat, role, message) + "\n";
        }
        return historyText;
    }

    /// <summary>
    /// Busca en Qdrant usando embeddings del prompt proporcionado
    /// </summary>
    private async Task<IReadOnlyList<Qdrant.Client.Grpc.ScoredPoint>?> SearchInQdrantAsync(string prompt)
    {
        var vector = await embeddingService.GenerateEmbeddingsAsync([prompt], null, CancellationToken.None);
        return await qdrantClient.SearchAsync(
            collectionName: Constants.Llm.CollectionName,
            vector: vector[0].ToArray(),
            limit: Constants.Llm.DefaultSearchLimit
        );
    }

    /// <summary>
    /// Busca en memoria semántica y devuelve solo el texto encontrado
    /// </summary>
    private async Task<string> SearchSemanticMemory(string prompt)
    {
        var searchPrompt = string.Format(Constants.Prompts.SemanticMemorySearchPrompt, prompt);
        var searchResult = await kernel.InvokePromptAsync(searchPrompt, arguments);

        string jsonString = searchResult.GetValue<string>() ?? string.Empty;
        var jsonStart = jsonString.IndexOf("{");
        var jsonEnd = jsonString.LastIndexOf("}");
        if (jsonStart >= 0 && jsonEnd > jsonStart)
        {
            jsonString = jsonString.Substring(jsonStart, jsonEnd - jsonStart + 1);
        }

        var dto = JsonSerializer.Deserialize<ChatResponseDto>(jsonString);
        return dto?.ResponseContent ?? string.Empty;
    }

    /// <summary>
    /// Formatea los resultados de Qdrant como texto
    /// </summary>
    private static async Task<string> FormatQdrantResults(IReadOnlyList<Qdrant.Client.Grpc.ScoredPoint>? results)
    {
        if (results == null || results.Count == 0)
            return string.Empty;

        var formattedResults = new List<string>();
        foreach (var point in results)
        {
            if (point.Payload.TryGetValue("text", out var textValue))
            {
                formattedResults.Add(textValue.ToString());
            }
        }

        if (formattedResults.Count == 0)
            return string.Empty;

        return string.Join("\n---\n", formattedResults);
    }

    /// <summary>
    /// Fusiona las fuentes de información en un contexto unificado
    /// </summary>
    private static string FuseContexts(string semanticContext, string qdrantContext)
    {
        var contexts = new List<string>();

        if (!string.IsNullOrWhiteSpace(semanticContext))
        {
            contexts.Add($"[Memoria Semántica]\n{semanticContext}");
        }

        if (!string.IsNullOrWhiteSpace(qdrantContext))
        {
            contexts.Add($"[Base de Conocimiento Qdrant]\n{qdrantContext}");
        }

        if (contexts.Count == 0)
            return "No se encontró información relevante en las fuentes disponibles.";

        return string.Join("\n\n", contexts);
    }

}

