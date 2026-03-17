
using System.Reflection;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using Nexus_api.Configuration;
using Nexus_api.Services;
using Nexus_api.Services.Interface;
using Nexus_api.Infrastructure.Pinecone;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.Configure<PineconeSettings>(builder.Configuration.GetSection("Pinecone"));

var appSettings = builder.Configuration.Get<AppSettings>() ?? throw new InvalidOperationException("Failed to bind AppSettings from configuration.");

 
    await using var tempMcpClient = await McpClient.CreateAsync(
        new StdioClientTransport(
            new StdioClientTransportOptions
            {
                Name = "nexus",
                Command = "/bin/bash",
                Arguments =
                [
                    "-c",
                    $"dotnet {appSettings.McpServer.RutaEjecucion} 2> stderr.log"
                ]
            }
        ),
        loggerFactory: LoggerFactory.Create(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Trace);
        })
    );

builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<PineconeSettings>>().Value;
    return PineconeClientFactory.Create(settings);
});

builder.Services.AddScoped<PineconeVectorDbRepository>();

var tools = tempMcpClient.ListToolsAsync(); 
builder.Services.AddControllers();
builder.Services.AddOpenApi();
var apiKey = appSettings?.Nexus?.ApiKey;
var modelId = appSettings?.Nexus?.ModelId;

if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(modelId))
{
    throw new InvalidOperationException("Nexus API key and model ID must be configured in appsettings.");
}

builder.Services.AddKernel()
    .AddOpenAIChatCompletion(modelId, apiKey)
    .Plugins.AddFromFunctions("Tools", tools.Result.Select(tools => tools.AsKernelFunction()));

//Acceso a la memoria semántica
builder.Services.AddKernelMemory<MemoryServerless>(kernelBuilder =>
{
    // Configuración del modelo de Embeddings
    var embeddingConfig = new OpenAIConfig
    {
        APIKey = apiKey,
        EmbeddingModel = "text-embedding-3-small",   //MODELO DE EMBEDDING
    };
    // Configuración del modelo de Chat
    var chatConfig = new OpenAIConfig
    {
        APIKey = apiKey,
        TextModel = modelId,   // tu modelo "gpt-5-nano" 
    };
    kernelBuilder
        .WithOpenAITextGeneration(chatConfig)
        .WithOpenAITextEmbeddingGeneration(embeddingConfig);
        //.WithQdrantMemoryDb("http://localhost:6333");

}, new KernelMemoryBuilderBuildOptions
{
    AllowMixingVolatileAndPersistentData = true
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // El puerto de tu frontend
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
//Inyecciones
builder.Services.AddScoped<AgentsServices>();
builder.Services.AddScoped<IDataEmbeddingServices, DataEmbeddingServices>();
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
try 
{
    app.MapControllers();
    app.MapScalarApiReference();
}
catch (System.Reflection.ReflectionTypeLoadException ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("!!! ERROR DE CARGA DE TIPOS !!!");
    foreach (var loaderEx in ex.LoaderExceptions)
    {
        if (loaderEx != null)
        {
            Console.WriteLine($"- {loaderEx.Message}");
            Console.WriteLine($"  Tipo: {loaderEx.GetType().Name}");
        }
    }
    Console.ResetColor();
    throw; // Relanzar para detener la ejecución
};
app.UseCors("AllowFrontend");
await app.RunAsync();
