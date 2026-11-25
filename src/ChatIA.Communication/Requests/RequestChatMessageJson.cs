namespace ChatIA.Communication.Requests
{
    /// <summary>
    /// Contrato de entrada do endpoint de chat (padrão Request*Json do CashFlow).
    /// </summary>
    public class RequestChatMessageJson
    {
        public string Message { get; set; } = string.Empty;
    }
}
