namespace ChatIA.Domain.Services.Ai
{
    /// <summary>
    /// Uma mensagem no histórico da conversa (contexto enviado ao LLM).
    /// Role: "user" ou "assistant".
    /// </summary>
    public class ChatTurn
    {
        public string Role { get; }
        public string Content { get; }

        public ChatTurn(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }
}
