using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Nexus_api.Infrastructure.Utils;

public static class Constants
{
    public static class Llm
    {
        public const string CollectionName = "pdfs";
        public const string MemoryCacheKeyPrefix = "conversation_history_";
        public const int HistoryCacheDurationMinutes = 30;
        public const int DefaultSearchLimit = 5;
        public const int MaxHistoryMessages = 10;
    }

    public static class Prompts
    {
        public const string ChatSystemPrompt = """
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
            """;

        public const string SemanticMemorySearchPrompt = """
            Busca en tu memoria semántica información relevante para responder a la siguiente pregunta del usuario: {0}
            Devuelve solo el texto encontrado sin agregar nada más.
            Regresa en estructura JSON: FoundInSemanticMemory (booleano) y ResponseContent (string con el texto encontrado o vacío si no se encontró nada).
            """;

        public const string ContextBuilderTemplate = """
            Historial de la conversación:
            {0}

            Información de la base de conocimiento:
            {1}

            Pregunta del usuario: {2}
            """;
    }

    public static class Formatting
    {
        public const string HistoryEntryFormat = "{0}: {1}";
        public const string HistoryHeader = "Historial de conversación:";
        public const string EmptyHistory = "";
    }

    public static class ExecutionSettings
    {
        public static OpenAIPromptExecutionSettings CreateChatSettings() => new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ChatSystemPrompt = Prompts.ChatSystemPrompt
        };

        public static KernelArguments CreateKernelArguments() => new(CreateChatSettings());
    }
}
