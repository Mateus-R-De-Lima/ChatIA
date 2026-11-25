namespace ChatIA.Communication.Requests
{
    /// <summary>
    /// Contrato de entrada do endpoint de chat (padrão Request*Json do CashFlow).
    /// History é opcional: permite que o modelo "lembre" da conversa atual.
    /// </summary>
    public class RequestChatMessageJson
    {
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Mensagens anteriores (sem a mensagem atual). Pode ser nulo ou vazio.
        /// </summary>
        public List<RequestChatHistoryItemJson>? History { get; set; }
    }
}
