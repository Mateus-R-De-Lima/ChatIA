using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;

namespace ChatIA.Infrastructure.Mcp
{
    /// <summary>
    /// Cliente MCP via stdio: sobe o ChatIA.McpServer como processo filho e lista as tools.
    /// </summary>
    internal sealed class McpToolProvider : IMcpToolProvider, IAsyncDisposable
    {
        private readonly McpSettings _settings;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<McpToolProvider> _logger;
        private readonly SemaphoreSlim _gate = new(1, 1);

        private McpClient? _client;
        private IList<AITool>? _cachedTools;

        public McpToolProvider(
            IOptions<McpSettings> settings,
            IConfiguration configuration,
            IHostEnvironment environment,
            ILogger<McpToolProvider> logger)
        {
            _settings = settings.Value;
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        public async Task<IList<AITool>> GetToolsAsync(CancellationToken cancellationToken = default)
        {
            if (!_settings.Enabled)
            {
                return [];
            }

            if (_cachedTools is not null)
            {
                return _cachedTools;
            }

            await _gate.WaitAsync(cancellationToken);
            try
            {
                if (_cachedTools is not null)
                {
                    return _cachedTools;
                }

                _client ??= await CreateClientAsync(cancellationToken);
                var mcpTools = await _client.ListToolsAsync(cancellationToken: cancellationToken);
                _cachedTools = mcpTools.Cast<AITool>().ToList();

                _logger.LogInformation(
                    "MCP: {Count} tool(s) disponíveis: {Tools}",
                    _cachedTools.Count,
                    string.Join(", ", _cachedTools.Select(tool => tool.Name)));

                return _cachedTools;
            }
            finally
            {
                _gate.Release();
            }
        }

        private async Task<McpClient> CreateClientAsync(CancellationToken cancellationToken)
        {
            var projectPath = ResolveServerProjectPath();
            if (!File.Exists(projectPath))
            {
                throw new FileNotFoundException(
                    $"Projeto do servidor MCP não encontrado: {projectPath}. Ajuste Settings:Mcp:ServerProjectPath.");
            }

            var mongoConnection = _configuration.GetConnectionString("MongoDb") ?? string.Empty;

            var transport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "ChatIA.McpServer",
                Command = _settings.Command,
                Arguments =
                [
                    "run",
                    "--project",
                    projectPath
                ],
                EnvironmentVariables = new Dictionary<string, string?>
                {
                    // Filho herda a connection string autenticada da Api.
                    ["ConnectionStrings__MongoDb"] = mongoConnection,
                    ["DOTNET_ENVIRONMENT"] = _environment.EnvironmentName,
                    ["ASPNETCORE_ENVIRONMENT"] = _environment.EnvironmentName
                }
            });

            _logger.LogInformation("MCP: iniciando servidor stdio em {Project}", projectPath);
            return await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);
        }

        private string ResolveServerProjectPath()
        {
            if (Path.IsPathRooted(_settings.ServerProjectPath))
            {
                return _settings.ServerProjectPath;
            }

            // Content root da Api = pasta do projeto ChatIA.Api
            var fromContentRoot = Path.GetFullPath(
                Path.Combine(_environment.ContentRootPath, _settings.ServerProjectPath));

            if (File.Exists(fromContentRoot))
            {
                return fromContentRoot;
            }

            // Fallback: bin/Debug/net9.0 → sobe até src/
            var fromBaseDirectory = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "ChatIA.McpServer", "ChatIA.McpServer.csproj"));

            return fromBaseDirectory;
        }

        public async ValueTask DisposeAsync()
        {
            if (_client is not null)
            {
                await _client.DisposeAsync();
                _client = null;
            }

            _gate.Dispose();
        }
    }
}
