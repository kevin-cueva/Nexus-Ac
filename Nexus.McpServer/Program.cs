using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;


var builder = Host.CreateEmptyApplicationBuilder(null);

//builder.Services.AddMemoryCache();

builder.Services.AddLogging(loggin =>
{
    loggin.AddConsole();
});

//Errores los envie a standard error
builder.Services.Configure<ConsoleLoggerOptions>(o =>
{
    o.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddMcpServer()
.WithStdioServerTransport()
.WithToolsFromAssembly();

await builder.Build().RunAsync();