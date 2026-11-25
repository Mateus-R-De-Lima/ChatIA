using ChatIA.Application;
using ChatIA.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Host do servidor MCP — segundo composition root (como a Api).
// Nesta etapa só registra as camadas; as tools MCP entram na Etapa 6.
var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("ChatIA.McpServer");
logger.LogInformation("ChatIA.McpServer iniciado. Tools MCP serão registradas na Etapa 6.");

await host.RunAsync();
