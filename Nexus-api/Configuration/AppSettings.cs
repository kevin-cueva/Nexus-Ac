namespace Nexus_api.Configuration;

public class NexusSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
}

public class PineconeSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
}

public class McpServerSettings
{
    public string RutaEjecucion { get; set; } = string.Empty;
    public string? Comando { get; set; }
}

public class AppSettings
{
    public NexusSettings Nexus { get; set; } = new();
    public PineconeSettings Pinecone { get; set; } = new();
    public McpServerSettings McpServer { get; set; } = new();
}
