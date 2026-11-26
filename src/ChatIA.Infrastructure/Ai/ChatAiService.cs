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
                var tools = await TryGetToolsAsync(cancellationToken);

                // 1) Perguntas claras de pedidos → MCP direto.
                //    Modelos locais 1B falham em tool_calls nativos e inventam JSON/números.
                if (tools.Count > 0
                    && PedidoIntentRouter.TryMatch(
                        userMessage,
                        conversationHistory,
                        out var routedTool,
                        out var routedArgs))
                {
                    logger.LogInformation(
                        "Roteando pergunta de pedidos para MCP '{Tool}'.",
                        routedTool);

                    var routed = await InvokeMcpToolAsync(
                        tools,
                        routedTool,
                        routedArgs,
                        cancellationToken);

                    if (!string.IsNullOrWhiteSpace(routed))
                    {
                        return routed.Trim();
                    }
                }

                // 2) Perguntas conceituais → só o LLM (sem tools).
                var messages = BuildMessages(userMessage, conversationHistory);
                var response = await chatClient.GetResponseAsync(
                    messages,
                    cancellationToken: cancellationToken);

                var text = response.Text?.Trim();

                // Fallback: modelos pequenos às vezes inventam JSON de tool no texto.
                if (PseudoToolCallParser.TryParse(text, out var toolName, out var arguments)
                    && tools.Count > 0)
                {
                    logger.LogWarning(
                        "Modelo devolveu tool call como texto ('{Tool}'). Executando via fallback MCP.",
                        toolName);

                    var toolResult = await InvokeMcpToolAsync(
                        tools,
                        toolName,
                        arguments,
                        cancellationToken);

                    if (!string.IsNullOrWhiteSpace(toolResult))
                    {
                        return toolResult.Trim();
                    }
                }

                if (string.IsNullOrWhiteSpace(text) || PseudoToolCallParser.LooksLikeToolDump(text))
                {
                    throw new ExternalServiceException(
                        "Não consegui obter uma resposta útil do modelo. Reformule a pergunta.");
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

        private async Task<string?> InvokeMcpToolAsync(
            IList<AITool> tools,
            string toolName,
            IReadOnlyDictionary<string, object?> arguments,
            CancellationToken cancellationToken)
        {
            var function = tools
                .OfType<AIFunction>()
                .FirstOrDefault(tool => tool.Name.Equals(toolName, StringComparison.OrdinalIgnoreCase));

            if (function is null)
            {
                logger.LogWarning("MCP: tool '{Tool}' não encontrada no catálogo.", toolName);
                return null;
            }

            try
            {
                var payload = new AIFunctionArguments(
                    arguments.ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value,
                        StringComparer.OrdinalIgnoreCase));

                var result = await function.InvokeAsync(payload, cancellationToken: cancellationToken);
                return result?.ToString();
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Falha ao executar tool MCP '{Tool}'.", toolName);
                throw new ExternalServiceException(
                    $"Falha ao executar a tool MCP '{toolName}': {ex.Message}");
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
                    if (PseudoToolCallParser.LooksLikeToolDump(turn.Content))
                    {
                        continue;
                    }

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
