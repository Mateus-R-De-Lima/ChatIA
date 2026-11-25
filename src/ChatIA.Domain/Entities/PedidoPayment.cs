namespace ChatIA.Domain.Entities
{
    /// <summary>
    /// Forma de pagamento do pedido (array payments no MongoDB).
    /// </summary>
    public class PedidoPayment
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public bool Prepaid { get; set; }
    }
}
