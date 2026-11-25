namespace ChatIA.Domain.Entities
{
    /// <summary>
    /// Pedido persistido em homologacao.Pedidos.
    /// Mapeamos só os campos necessários para consultas de leitura (Etapa 5+).
    /// Os nomes BSON reais (pedido_id, createdAt, etc.) ficam na Infrastructure.
    /// </summary>
    public class Pedido
    {
        public string Id { get; set; } = string.Empty;
        public string EmpresaId { get; set; } = string.Empty;
        public string PedidoId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Customer? Customer { get; set; }
        public IList<PedidoItem> Items { get; set; } = [];
        public IList<PedidoPayment> Payments { get; set; } = [];
        public decimal Total { get; set; }
        public string Type { get; set; } = string.Empty;
        public string SalesChannel { get; set; } = string.Empty;
        public int ShortReference { get; set; }
    }
}
