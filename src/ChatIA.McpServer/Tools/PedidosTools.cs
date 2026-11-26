using System.ComponentModel;
using System.Globalization;
using System.Text;
using ChatIA.Application.UseCases.Pedidos;
using ChatIA.Application.UseCases.Pedidos.GetByShortReference;
using ChatIA.Application.UseCases.Pedidos.GetCount;
using ChatIA.Application.UseCases.Pedidos.GetLatest;
using ChatIA.Application.UseCases.Pedidos.GetLatestDetail;
using ChatIA.Application.UseCases.Pedidos.GetMonthlyRevenue;
using ModelContextProtocol.Server;

namespace ChatIA.McpServer.Tools
{
    /// <summary>
    /// Tools MCP de leitura de pedidos (delivery / PDV).
    /// Só adapta protocolo; a regra fica nos use cases.
    /// </summary>
    [McpServerToolType]
    public class PedidosTools(
        IGetPedidosCountUseCase getPedidosCountUseCase,
        IGetLatestPedidosUseCase getLatestPedidosUseCase,
        IGetLatestPedidoDetailUseCase getLatestPedidoDetailUseCase,
        IGetPedidoByShortReferenceUseCase getPedidoByShortReferenceUseCase,
        IGetMonthlyRevenueUseCase getMonthlyRevenueUseCase)
    {
        [McpServerTool(Name = "get_pedidos_count")]
        [Description(
            "Conta pedidos. Ex.: 'quantos pedidos?', 'total de pedidos', 'quantidade de pedidos'.")]
        public async Task<string> GetPedidosCount(CancellationToken cancellationToken = default)
        {
            var result = await getPedidosCountUseCase.Execute(cancellationToken);
            return $"Total de pedidos cadastrados: {result.Count}";
        }

        [McpServerTool(Name = "get_latest_pedidos")]
        [Description(
            "Lista resumida dos pedidos mais recentes. Ex.: 'últimos 5 pedidos', 'pedidos recentes'.")]
        public async Task<string> GetLatestPedidos(
            [Description("Quantidade (1-50). Padrão: 10.")] int limit = 10,
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

        [McpServerTool(Name = "get_latest_pedido_detail")]
        [Description(
            "Detalhe completo do ÚLTIMO pedido (itens, cliente, pagamento, canal, tipo). " +
            "Ex.: 'qual o último pedido?', 'itens do último pedido', 'como pagou o último?'.")]
        public async Task<string> GetLatestPedidoDetail(CancellationToken cancellationToken = default)
        {
            var pedido = await getLatestPedidoDetailUseCase.Execute(cancellationToken);
            return pedido is null
                ? "Nenhum pedido encontrado."
                : PedidoDetailMapper.Format(pedido);
        }

        [McpServerTool(Name = "get_pedido_by_reference")]
        [Description(
            "Detalhe completo de um pedido pela ref. curta (#). Cobre itens, marca no nome/obs, " +
            "cliente, telefone, pagamento, total, tipo, canal. " +
            "Ex.: 'detalhes do pedido 12', 'itens do pedido #5', 'quem pediu o 3?', 'como pagou o 8?'.")]
        public async Task<string> GetPedidoByReference(
            [Description("short_reference do pedido (ex.: 12).")] int shortReference,
            CancellationToken cancellationToken = default)
        {
            var pedido = await getPedidoByShortReferenceUseCase.Execute(shortReference, cancellationToken);
            return pedido is null
                ? $"Pedido #{shortReference} não encontrado."
                : PedidoDetailMapper.Format(pedido);
        }

        [McpServerTool(Name = "get_monthly_revenue")]
        [Description(
            "Faturamento (soma dos totais) de um mês/ano. Ex.: 'faturamento de setembro 2025'.")]
        public async Task<string> GetMonthlyRevenue(
            [Description("Ano, ex.: 2025.")] int year,
            [Description("Mês 1-12.")] int month,
            CancellationToken cancellationToken = default)
        {
            var result = await getMonthlyRevenueUseCase.Execute(year, month, cancellationToken);
            var revenue = result.Revenue.ToString("0.00", CultureInfo.InvariantCulture);
            return $"Faturamento de {result.Month:00}/{result.Year}: {revenue}";
        }
    }
}
