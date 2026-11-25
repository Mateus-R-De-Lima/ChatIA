using ChatIA.Communication.Requests;
using ChatIA.Communication.Responses;
using ChatIA.Domain.Services.Ai;
using ChatIA.Exception.ExceptionsBase;

namespace ChatIA.Application.UseCases.Chat.SendMessage
{
    /// <summary>
    /// Caso de uso do chat.
    /// Valida a entrada e delega a geração da resposta ao IChatAiService (Domain).
    /// </summary>
    public class SendChatMessageUseCase(IChatAiService chatAiService) : ISendChatMessageUseCase
    {
        public async Task<ResponseChatMessageJson> Execute(RequestChatMessageJson request)
        {
            Validate(request);

            var history = MapHistory(request.History);

            var reply = await chatAiService.GetReplyAsync(
                request.Message.Trim(),
                history);

            return new ResponseChatMessageJson
            {
                Reply = reply
            };
        }

        private static void Validate(RequestChatMessageJson request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                throw new ErrorOnValidationException(["A mensagem não pode ser vazia."]);
            }
        }

        private static List<ChatTurn>? MapHistory(List<RequestChatHistoryItemJson>? history)
        {
            if (history is null || history.Count == 0)
            {
                return null;
            }

            // Limita o contexto para não estourar tokens / custo nas primeiras etapas.
            const int maxTurns = 12;

            return history
                .Where(item =>
                    !string.IsNullOrWhiteSpace(item.Content) &&
                    (item.Role.Equals("user", StringComparison.OrdinalIgnoreCase) ||
                     item.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase)))
                .TakeLast(maxTurns)
                .Select(item => new ChatTurn(item.Role.Trim().ToLowerInvariant(), item.Content.Trim()))
                .ToList();
        }
    }
}
