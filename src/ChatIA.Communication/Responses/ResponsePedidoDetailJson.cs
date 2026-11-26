namespace ChatIA.Communication.Responses
{
    public class ResponsePedidoDetailJson
    {
        public string Id { get; set; } = string.Empty;
        public string PedidoId { get; set; } = string.Empty;
        public string ShortReference { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerDocument { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Type { get; set; } = string.Empty;
        public string SalesChannel { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<ResponsePedidoItemJson> Items { get; set; } = [];
        public List<ResponsePedidoPaymentJson> Payments { get; set; } = [];
    }

    public class ResponsePedidoItemJson
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public string Observation { get; set; } = string.Empty;
    }

    public class ResponsePedidoPaymentJson
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public bool Prepaid { get; set; }
    }
}
