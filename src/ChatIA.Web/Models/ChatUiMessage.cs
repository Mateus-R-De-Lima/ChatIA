namespace ChatIA.Web.Models
{
    /// <summary>
    /// Modelo só de UI para renderizar o histórico na tela.
    /// Não substitui os DTOs da Api (Request/Response*Json).
    /// </summary>
    public class ChatUiMessage
    {
        public required string Role { get; init; }
        public required string Content { get; init; }
        public DateTime SentAt { get; init; } = DateTime.Now;

        public bool IsUser => Role.Equals("user", StringComparison.OrdinalIgnoreCase);
    }
}
