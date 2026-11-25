namespace ChatIA.Communication.Responses
{
    public class ResponsePedidoShortJson
    {
        public string Id { get; set; } = string.Empty;
        public string ShortReference { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Type { get; set; } = string.Empty;
        public string SalesChannel { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
