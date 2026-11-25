using System.ClientModel;
using ChatIA.Domain.Services.Ai;
using ChatIA.Infrastructure.Ai;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;

namespace ChatIA.Infrastructure
{
    /// <summary>
    /// Composition da camada Infrastructure — mesmo padrão do CashFlow.
    /// </summary>
    public static class DependencyInjectionExtension
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            AddAiServices(services, configuration);
        }

        private static void AddAiServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AiSettings>(configuration.GetSection(AiSettings.SectionName));

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

                // Endpoint vazio → OpenAI oficial.
                // Endpoint preenchido → qualquer API compatível (útil para aprendizado local).
                OpenAIClient client = string.IsNullOrWhiteSpace(settings.Endpoint)
                    ? new OpenAIClient(settings.ApiKey)
                    : new OpenAIClient(
                        new ApiKeyCredential(settings.ApiKey),
                        new OpenAIClientOptions { Endpoint = new Uri(settings.Endpoint) });

                return client.GetChatClient(model).AsIChatClient();
            });

            services.AddScoped<IChatAiService, ChatAiService>();
        }
    }
}
