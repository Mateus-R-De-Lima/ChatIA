using ChatIA.Application;
using ChatIA.Infrastructure;
using ChatIA.McpServer.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Host MCP (stdio) — segundo composition root.
// stdout = protocolo MCP (JSON-RPC). Logs devem ir para stderr.
var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// McpServer não precisa de LLM — só Mongo + use cases de leitura.
builder.Services.AddInfrastructure(builder.Configuration, registerAi: false);
builder.Services.AddApplication();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<PedidosTools>();

await builder.Build().RunAsync();
