using ChatIA.Domain.Services.Ai;
using ChatIA.Exception.ExceptionsBase;
using ChatIA.Infrastructure.Mcp;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChatIA.Infrastructure.Ai
{
    /// <summary>
    /// Adaptador Domain.IChatAiService → IChatClient (+ tools MCP quando disponíveis).
    /// </summary>
    internal class ChatAiService(
        IChatClient chatClient,
        IMcpToolProvider mcpToolProvider,
        IOptions<AiSettings> options,
        ILogger<ChatAiService> logger) : IChatAiService
    {
        public async Task<string> GetReplyAsync(
            string userMessage,
            IReadOnlyList<ChatTurn>? conversationHistory = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var messages = BuildMessages(userMessage, conversationHistory);
                var tools = await TryGetToolsAsync(cancellationToken);

                var chatOptions = new ChatOptions
                {
                    Tools = tools.Count == 0 ? null : [.. tools]
                };

                // Com UseFunctionInvocation no IChatClient, o middleware:
                // 1) detecta tool call do modelo
                // 2) executa a tool MCP
                // 3) devolve o resultado ao modelo
                // 4) retorna a resposta final em linguagem natural
                var response = await chatClient.GetResponseAsync(
                    messages,
                    chatOptions,
                    cancellationToken);

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
                throw new ExternalServiceException(
                    $"Falha ao comunicar com o modelo de IA: {ex.Message}");
            }
        }

        private async Task<IList<AITool>> TryGetToolsAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await mcpToolProvider.GetToolsAsync(cancellationToken);
            }
            catch (System.Exception ex)
            {
                // Chat continua sem tools (cenário A: só conhecimento do modelo).
                logger.LogWarning(ex, "MCP indisponível; o chat seguirá sem tools.");
                return [];
            }
        }

        private List<ChatMessage> BuildMessages(
            string userMessage,
            IReadOnlyList<ChatTurn>? conversationHistory)
        {
            var settings = options.Value;
            var messages = new List<ChatMessage>
            {
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
