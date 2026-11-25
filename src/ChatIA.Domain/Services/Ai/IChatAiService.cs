namespace ChatIA.Domain.Services.Ai
{
    /// <summary>
    /// Contrato do serviço de IA (mesmo padrão de IAccessTokenGenarator no CashFlow):
    /// a abstração fica no Domain; a implementação concreta fica na Infrastructure.
    /// </summary>
    public interface IChatAiService
    {
        /// <summary>
        /// Envia a mensagem do usuário (e o histórico opcional) ao modelo e devolve a resposta.
        /// </summary>
        Task<string> GetReplyAsync(
            string userMessage,
            IReadOnlyList<ChatTurn>? conversationHistory = null,
            CancellationToken cancellationToken = default);
    }
}
