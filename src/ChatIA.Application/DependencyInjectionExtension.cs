using ChatIA.Application.UseCases.Chat.SendMessage;
using Microsoft.Extensions.DependencyInjection;

namespace ChatIA.Application
{
    /// <summary>
    /// Composition da camada Application — mesmo padrão do CashFlow.
    /// Os hosts (Api e McpServer) chamam AddApplication() no Program.
    /// </summary>
    public static class DependencyInjectionExtension
    {
        public static void AddApplication(this IServiceCollection services)
        {
            AddUseCases(services);
        }

        private static void AddUseCases(IServiceCollection services)
        {
            services.AddScoped<ISendChatMessageUseCase, SendChatMessageUseCase>();
        }
    }
}

