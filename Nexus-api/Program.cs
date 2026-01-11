
using System.Reflection;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;

var builder = WebApplication.CreateBuilder(args);

var apiKey = builder.Configuration["Nexus:ApiKey"];
var modelId = builder.Configuration["Nexus:ModelId"];
var rutaMcp = builder.Configuration["McpServer:RutaEjecucion"];

await using McpClient mcpClient =
    await McpClient.CreateAsync(
        new StdioClientTransport(
            new StdioClientTransportOptions
{
    Name = "nexus",
    Command = "/bin/bash",
    Arguments =
    [
        "-c",
        $"dotnet {rutaMcp} 2> stderr.log"
    ]
}
        ),
        loggerFactory: LoggerFactory.Create(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Trace);
        })
    );

var tools = mcpClient.ListToolsAsync(); 
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddKernel()
    .AddOpenAIChatCompletion(modelId!, apiKey!)
    .Plugins.AddFromFunctions("Tools", tools.Result.Select(tools => tools.AsKernelFunction()));

//Acceso a la memoria semántica
builder.Services.AddKernelMemory<MemoryServerless>(kernelBuilder =>
{
    // Configuración del modelo de Embeddings
    var embeddingConfig = new OpenAIConfig
    {
        APIKey = apiKey!,
        EmbeddingModel = "text-embedding-3-small",   //MODELO DE EMBEDDING
    };
    // Configuración del modelo de Chat
    var chatConfig = new OpenAIConfig
    {
        APIKey = apiKey!,
        TextModel = modelId!,   // tu modelo "gpt-5-nano" 
    };
    kernelBuilder
        .WithOpenAITextGeneration(chatConfig)
        .WithOpenAITextEmbeddingGeneration(embeddingConfig)
        .WithQdrantMemoryDb("http://localhost:6333");

}, new KernelMemoryBuilderBuildOptions
{
    AllowMixingVolatileAndPersistentData = true
});

//Inyecciones
builder.Services.AddMemoryCache();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
//Importa automáticamente una página web a la memoria semántica (Kernel Memory)
// al iniciar la aplicación
if (bool.TryParse(Environment.GetEnvironmentVariable(
    "VectorizeAtStartup"), out bool vectorizeAtStartup) && vectorizeAtStartup)
{
    using var scope = app.Services.CreateScope();
    try
    {
        //Accedo a los metodos de kernel memory
        var kernelMemory = scope.ServiceProvider.GetRequiredService<IKernelMemory>();
        //await kernelMemory.ImportWebPageAsync("https://faburobotics.com/");
        //await kernelMemory.ImportDocumentAsync("FB-02_Fact_Sheet_Spanish.pdf", "fb-02"); 
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al importar la web: {ex.Message}");
    }
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
