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
            // Etapa 2: ainda não há use cases.
            // Nas próximas etapas registraremos aqui, por exemplo:
            // services.AddScoped<ISendChatMessageUseCase, SendChatMessageUseCase>();
        }
    }
}
