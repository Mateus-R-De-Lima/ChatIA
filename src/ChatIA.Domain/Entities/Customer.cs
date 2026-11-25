namespace ChatIA.Domain.Entities
{
    /// <summary>
    /// Cliente embutido no documento de Pedido (coleção Pedidos).
    /// </summary>
    public class Customer
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? TaxPayerIdentificationNumber { get; set; }
    }
}
