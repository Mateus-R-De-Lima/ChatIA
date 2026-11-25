namespace ChatIA.Communication.Requests
{
    /// <summary>
    /// Um turno anterior da conversa, usado como contexto para o LLM.
    /// </summary>
    public class RequestChatHistoryItemJson
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
