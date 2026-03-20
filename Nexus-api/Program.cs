
using System.Reflection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using ModelContextProtocol.Client;
using Nexus_api.Services;
using Nexus_api.Services.Interface;
using Qdrant.Client;
using Scalar.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;


var builder = WebApplication.CreateBuilder(args);

var apiKey = builder.Configuration["Nexus:ApiKey"];
var modelId = builder.Configuration["Nexus:ModelId"];
var rutaMcp = builder.Configuration["McpServer:RutaEjecucion"];
var qdrantEndpoint = builder.Configuration["Qdrant:Endpoint"];
var qdrantApiKey = builder.Configuration["Qdrant:ApiKey"];

// IMPORTANTE: Deshabilitar el proxy del sistema para las conexiones gRPC a Qdrant
// Los proxies HTTP corporativos no son compatibles con gRPC/HTTP2
// Esta configuración debe ejecutarse ANTES de crear cualquier cliente HTTP/gRPC
AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", true);
HttpClient.DefaultProxy = new WebProxy()
{
    BypassList = [qdrantEndpoint ?? "*"]
};

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
    .AddOpenAITextEmbeddingGeneration("text-embedding-3-small", apiKey!)
    .Plugins.AddFromFunctions("Tools", tools.Result.Select(tools => tools.AsKernelFunction()));
builder.Services.AddSingleton(sp => new QdrantClient(qdrantEndpoint!, 6334!, true, qdrantApiKey!));

//Acceso a la memoria semántica

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
