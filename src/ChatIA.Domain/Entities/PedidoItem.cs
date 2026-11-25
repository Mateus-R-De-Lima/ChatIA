namespace ChatIA.Domain.Entities
{
    /// <summary>
    /// Item do pedido (array items no MongoDB).
    /// </summary>
    public class PedidoItem
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public string? Observation { get; set; }
        public string? InternalId { get; set; }
    }
}
