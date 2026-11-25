using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatIA.Infrastructure
{
    /// <summary>
    /// Composition da camada Infrastructure — mesmo padrão do CashFlow.
    /// Aqui entrarão MongoDB, cliente LLM e cliente MCP.
    /// </summary>
    public static class DependencyInjectionExtension
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Etapa 2: apenas o "esqueleto" da DI.
            // Etapas futuras:
            // - AddMongoDb(services, configuration);
            // - AddAiServices(services, configuration);
            // - AddMcpClient(services, configuration);
        }
    }
}
