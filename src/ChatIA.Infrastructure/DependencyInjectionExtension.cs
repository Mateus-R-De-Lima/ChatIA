using System.ClientModel;
using ChatIA.Domain.Repositories.Pedidos;
using ChatIA.Domain.Services.Ai;
using ChatIA.Infrastructure.Ai;
using ChatIA.Infrastructure.DataAccess;
using ChatIA.Infrastructure.DataAccess.Repositories;
using ChatIA.Infrastructure.Mcp;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using OpenAI;

namespace ChatIA.Infrastructure
{
    /// <summary>
    /// Composition da camada Infrastructure — mesmo padrão do CashFlow.
    /// </summary>
    public static class DependencyInjectionExtension
    {
        /// <param name="registerAi">
        /// true na Api (chat + LLM + cliente MCP). false no McpServer (só Mongo/repositórios).
        /// </param>
        public static void AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            bool registerAi = true)
        {
            if (registerAi)
            {
                AddAiServices(services, configuration);
            }

            AddMongoDb(services, configuration);
            AddRepositories(services);
        }

        private static void AddAiServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AiSettings>(configuration.GetSection(AiSettings.SectionName));
            services.Configure<McpSettings>(configuration.GetSection(McpSettings.SectionName));

            services.AddSingleton<IMcpToolProvider, McpToolProvider>();

            services.AddSingleton<IChatClient>(sp =>
            {
                var settings = configuration.GetSection(AiSettings.SectionName).Get<AiSettings>()
                    ?? new AiSettings();

                if (string.IsNullOrWhiteSpace(settings.ApiKey))
                {
                    throw new InvalidOperationException(
                        "Configure Settings:Ai:ApiKey via User Secrets ou variável de ambiente. " +
                        "Ex.: dotnet user-secrets set \"Settings:Ai:ApiKey\" \"sua-chave\" --project src/ChatIA.Api");
                }

                var model = string.IsNullOrWhiteSpace(settings.Model)
                    ? "gpt-4o-mini"
                    : settings.Model;

                OpenAIClient client = string.IsNullOrWhiteSpace(settings.Endpoint)
                    ? new OpenAIClient(settings.ApiKey)
                    : new OpenAIClient(
                        new ApiKeyCredential(settings.ApiKey),
                        new OpenAIClientOptions { Endpoint = new Uri(settings.Endpoint) });

                // UseFunctionInvocation = loop automático de tool calling.
                return new ChatClientBuilder(client.GetChatClient(model).AsIChatClient())
                    .UseFunctionInvocation()
                    .Build();
            });

            services.AddScoped<IChatAiService, ChatAiService>();
        }

        private static void AddMongoDb(IServiceCollection services, IConfiguration configuration)
        {
            MongoClassMaps.Register();

            services.Configure<MongoSettings>(configuration.GetSection(MongoSettings.SectionName));

            var connectionString = configuration.GetConnectionString("MongoDb");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Configure ConnectionStrings:MongoDb. " +
                    "Sem auth: mongodb://localhost:27017 | Com auth: mongodb://usuario:senha@localhost:27017. " +
                    "Prefira User Secrets no host (Api ou McpServer).");
            }

            services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
            services.AddSingleton<MongoDbContext>();
        }

        private static void AddRepositories(IServiceCollection services)
        {
            services.AddScoped<IPedidoReadOnlyRepository, PedidoRepository>();
        }
    }
}
