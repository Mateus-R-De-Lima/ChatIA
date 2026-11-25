using ChatIA.Application.UseCases.Chat.SendMessage;
using ChatIA.Application.UseCases.Pedidos.GetCount;
using ChatIA.Application.UseCases.Pedidos.GetLatest;
using ChatIA.Application.UseCases.Pedidos.GetMonthlyRevenue;
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

            services.AddScoped<IGetPedidosCountUseCase, GetPedidosCountUseCase>();
            services.AddScoped<IGetLatestPedidosUseCase, GetLatestPedidosUseCase>();
            services.AddScoped<IGetMonthlyRevenueUseCase, GetMonthlyRevenueUseCase>();
        }
    }
}
