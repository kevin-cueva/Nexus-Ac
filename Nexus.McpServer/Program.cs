using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

var builder = Host.CreateEmptyApplicationBuilder(null);

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
});

builder.Services.Configure<ConsoleLoggerOptions>(o =>
{
    o.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly(typeof(Nexus.McpServer.Plugins.Prueba).Assembly);

await builder.Build().RunAsync();
