using ChatIA.Application.UseCases.Chat.SendMessage;
using ChatIA.Application.UseCases.Pedidos.GetByShortReference;
using ChatIA.Application.UseCases.Pedidos.GetCount;
using ChatIA.Application.UseCases.Pedidos.GetLatest;
using ChatIA.Application.UseCases.Pedidos.GetLatestDetail;
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
        /// <param name="registerChat">
        /// true na Api. false no McpServer (não precisa de IChatAiService).
        /// </param>
        public static void AddApplication(this IServiceCollection services, bool registerChat = true)
        {
            if (registerChat)
            {
                services.AddScoped<ISendChatMessageUseCase, SendChatMessageUseCase>();
            }

            services.AddScoped<IGetPedidosCountUseCase, GetPedidosCountUseCase>();
            services.AddScoped<IGetLatestPedidosUseCase, GetLatestPedidosUseCase>();
            services.AddScoped<IGetLatestPedidoDetailUseCase, GetLatestPedidoDetailUseCase>();
            services.AddScoped<IGetPedidoByShortReferenceUseCase, GetPedidoByShortReferenceUseCase>();
            services.AddScoped<IGetMonthlyRevenueUseCase, GetMonthlyRevenueUseCase>();
        }
    }
}
