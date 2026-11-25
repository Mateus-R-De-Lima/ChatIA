using System.ComponentModel;
using System.Globalization;
using System.Text;
using ChatIA.Application.UseCases.Pedidos.GetCount;
using ChatIA.Application.UseCases.Pedidos.GetLatest;
using ChatIA.Application.UseCases.Pedidos.GetMonthlyRevenue;
using ModelContextProtocol.Server;

namespace ChatIA.McpServer.Tools
{
    /// <summary>
    /// Tools MCP de leitura de pedidos.
    /// Este projeto é só o adaptador do protocolo: a regra fica nos use cases (Application).
    /// </summary>
    [McpServerToolType]
    public class PedidosTools(
        IGetPedidosCountUseCase getPedidosCountUseCase,
        IGetLatestPedidosUseCase getLatestPedidosUseCase,
        IGetMonthlyRevenueUseCase getMonthlyRevenueUseCase)
    {
        [McpServerTool(Name = "get_pedidos_count")]
        [Description("Retorna quantos pedidos estão cadastrados no MongoDB (coleção Pedidos). Use para perguntas como 'quantos pedidos existem?'.")]
        public async Task<string> GetPedidosCount(CancellationToken cancellationToken = default)
        {
            var result = await getPedidosCountUseCase.Execute(cancellationToken);
            return $"Total de pedidos cadastrados: {result.Count}";
        }

        [McpServerTool(Name = "get_latest_pedidos")]
        [Description("Lista os pedidos mais recentes do MongoDB. Use para perguntas como 'quais são os últimos pedidos?'.")]
        public async Task<string> GetLatestPedidos(
            [Description("Quantidade de pedidos a retornar (entre 1 e 50). Padrão: 10.")] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await getLatestPedidosUseCase.Execute(limit, cancellationToken);

            if (result.Pedidos.Count == 0)
            {
                return "Nenhum pedido encontrado.";
            }

            var builder = new StringBuilder();
            builder.AppendLine($"Últimos {result.Pedidos.Count} pedidos:");

            foreach (var pedido in result.Pedidos)
            {
                var total = pedido.Total.ToString("0.00", CultureInfo.InvariantCulture);
                builder.AppendLine(
                    $"- Ref #{pedido.ShortReference} | Cliente: {pedido.CustomerName} | " +
                    $"Total: {total} | Canal: {pedido.SalesChannel} | " +
                    $"Tipo: {pedido.Type} | Data: {pedido.CreatedAt:yyyy-MM-dd HH:mm} UTC");
            }

            return builder.ToString().TrimEnd();
        }

        [McpServerTool(Name = "get_monthly_revenue")]
        [Description("Calcula o faturamento (soma do campo total) dos pedidos em um mês/ano. Use para perguntas como 'qual o faturamento do mês?'.")]
        public async Task<string> GetMonthlyRevenue(
            [Description("Ano, por exemplo 2025.")] int year,
            [Description("Mês de 1 a 12.")] int month,
            CancellationToken cancellationToken = default)
        {
            var result = await getMonthlyRevenueUseCase.Execute(year, month, cancellationToken);
            var revenue = result.Revenue.ToString("0.00", CultureInfo.InvariantCulture);
            return $"Faturamento de {result.Month:00}/{result.Year}: {revenue}";
        }
    }
}
