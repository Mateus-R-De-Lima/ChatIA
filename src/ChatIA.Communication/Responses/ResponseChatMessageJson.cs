namespace ChatIA.Communication.Responses
{
    /// <summary>
    /// Contrato de saída do endpoint de chat (padrão Response*Json do CashFlow).
    /// </summary>
    public class ResponseChatMessageJson
    {
        public string Reply { get; set; } = string.Empty;
    }
}
