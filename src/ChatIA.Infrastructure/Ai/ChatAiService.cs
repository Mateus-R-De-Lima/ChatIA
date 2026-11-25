using ChatIA.Domain.Services.Ai;
using ChatIA.Exception.ExceptionsBase;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace ChatIA.Infrastructure.Ai
{
    /// <summary>
    /// Adaptador: Domain.IChatAiService → Microsoft.Extensions.AI.IChatClient.
    /// A Application não conhece OpenAI; só este contrato.
    /// </summary>
    internal class ChatAiService(
        IChatClient chatClient,
        IOptions<AiSettings> options) : IChatAiService
    {
        public async Task<string> GetReplyAsync(
            string userMessage,
            IReadOnlyList<ChatTurn>? conversationHistory = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var messages = BuildMessages(userMessage, conversationHistory);

                // GetResponseAsync envia o contexto completo ao modelo e devolve a resposta.
                var response = await chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);

                var text = response.Text?.Trim();
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new ExternalServiceException("O modelo de IA retornou uma resposta vazia.");
                }

                return text;
            }
            catch (ExternalServiceException)
            {
                throw;
            }
            catch (System.Exception ex)
            {
                // Qualificamos System.Exception porque o projeto referencia o namespace ChatIA.Exception.
                throw new ExternalServiceException(
                    $"Falha ao comunicar com o modelo de IA: {ex.Message}");
            }
        }

        private List<ChatMessage> BuildMessages(
            string userMessage,
            IReadOnlyList<ChatTurn>? conversationHistory)
        {
            var settings = options.Value;
            var messages = new List<ChatMessage>
            {
                // System prompt: instruções fixas do assistente (não é histórico do usuário).
                new(ChatRole.System, settings.SystemPrompt)
            };

            if (conversationHistory is not null)
            {
                foreach (var turn in conversationHistory)
                {
                    messages.Add(ToChatMessage(turn));
                }
            }

            messages.Add(new ChatMessage(ChatRole.User, userMessage));
            return messages;
        }

        private static ChatMessage ToChatMessage(ChatTurn turn)
        {
            if (turn.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase))
            {
                return new ChatMessage(ChatRole.Assistant, turn.Content);
            }

            return new ChatMessage(ChatRole.User, turn.Content);
        }
    }
}
