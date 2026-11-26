using System.Globalization;
using System.Text;
using ChatIA.Communication.Responses;
using ChatIA.Domain.Entities;

namespace ChatIA.Application.UseCases.Pedidos
{
    /// <summary>
    /// Converte Pedido → DTO detalhado e formata texto para o chat/MCP.
    /// </summary>
    public static class PedidoDetailMapper
    {
        public static ResponsePedidoDetailJson ToDetail(Pedido pedido) => new()
        {
            Id = pedido.Id,
            PedidoId = pedido.PedidoId,
            ShortReference = pedido.ShortReference.ToString(),
            CustomerName = pedido.Customer?.Name ?? string.Empty,
            CustomerPhone = pedido.Customer?.Phone ?? string.Empty,
            CustomerDocument = pedido.Customer?.TaxPayerIdentificationNumber ?? string.Empty,
            Total = pedido.Total,
            Type = pedido.Type,
            SalesChannel = pedido.SalesChannel,
            CreatedAt = pedido.CreatedAt,
            Items = pedido.Items
                .Select(item => new ResponsePedidoItemJson
                {
                    Name = item.Name,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Total = item.Total,
                    Observation = item.Observation ?? string.Empty
                })
                .ToList(),
            Payments = pedido.Payments
                .Select(payment => new ResponsePedidoPaymentJson
                {
                    Name = payment.Name,
                    Code = payment.Code,
                    Value = payment.Value,
                    Prepaid = payment.Prepaid
                })
                .ToList()
        };

        public static string Format(ResponsePedidoDetailJson pedido)
        {
            var total = Money(pedido.Total);
            var builder = new StringBuilder();

            builder.AppendLine($"Pedido #{pedido.ShortReference}");
            if (!string.IsNullOrWhiteSpace(pedido.PedidoId))
            {
                builder.AppendLine($"ID externo: {pedido.PedidoId}");
            }

            builder.AppendLine($"Cliente: {NullToDash(pedido.CustomerName)}");
            builder.AppendLine($"Telefone: {NullToDash(pedido.CustomerPhone)}");
            builder.AppendLine($"Documento: {NullToDash(pedido.CustomerDocument)}");
            builder.AppendLine($"Total: {total}");
            builder.AppendLine($"Tipo: {NullToDash(DescribeType(pedido.Type))}");
            builder.AppendLine($"Canal: {NullToDash(pedido.SalesChannel)}");
            builder.AppendLine($"Data: {pedido.CreatedAt:yyyy-MM-dd HH:mm} UTC");

            builder.AppendLine();
            builder.AppendLine($"Itens ({pedido.Items.Count}):");
            if (pedido.Items.Count == 0)
            {
                builder.AppendLine("- (nenhum item cadastrado)");
            }
            else
            {
                foreach (var item in pedido.Items)
                {
                    builder.AppendLine(
                        $"- {item.Name} | qtd {item.Quantity} | unit {Money(item.Price)} | total {Money(item.Total)}");
                    if (!string.IsNullOrWhiteSpace(item.Observation))
                    {
                        builder.AppendLine($"  obs: {item.Observation}");
                    }
                }
            }

            builder.AppendLine();
            builder.AppendLine($"Pagamentos ({pedido.Payments.Count}):");
            if (pedido.Payments.Count == 0)
            {
                builder.AppendLine("- (nenhum pagamento cadastrado)");
            }
            else
            {
                foreach (var payment in pedido.Payments)
                {
                    var prepaid = payment.Prepaid ? "pré-pago" : "não pré-pago";
                    builder.AppendLine(
                        $"- {NullToDash(payment.Name)} ({NullToDash(payment.Code)}) | {Money(payment.Value)} | {prepaid}");
                }
            }

            builder.AppendLine();
            builder.AppendLine(
                "Nota: o banco não tem campo separado de 'marca'. " +
                "Marca/sabor/tamanho, quando existirem, costumam aparecer no nome do item ou na observação.");

            return builder.ToString().TrimEnd();
        }

        private static string Money(decimal value) =>
            value.ToString("0.00", CultureInfo.InvariantCulture);

        private static string NullToDash(string? value) =>
            string.IsNullOrWhiteSpace(value) ? "-" : value;

        private static string DescribeType(string type) => type.ToUpperInvariant() switch
        {
            "TAKE" => "TAKE (retirada)",
            "DELIVERY" => "DELIVERY (entrega)",
            "INDOOR" or "DINE_IN" => $"{type} (consumo no local)",
            _ => type
        };
    }
}
