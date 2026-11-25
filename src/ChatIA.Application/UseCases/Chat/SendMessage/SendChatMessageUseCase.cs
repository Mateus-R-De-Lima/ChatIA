using ChatIA.Communication.Requests;
using ChatIA.Communication.Responses;
using ChatIA.Exception.ExceptionsBase;

namespace ChatIA.Application.UseCases.Chat.SendMessage
{
    /// <summary>
    /// Caso de uso do chat.
    /// Etapa 3: resposta simulada (sem LLM).
    /// Etapa 4: este mesmo contrato passará a chamar o serviço de IA.
    /// </summary>
    public class SendChatMessageUseCase : ISendChatMessageUseCase
    {
        public async Task<ResponseChatMessageJson> Execute(RequestChatMessageJson request)
        {
            Validate(request);

            // Simula latência de rede/modelo para o indicador de carregamento no Blazor.
            await Task.Delay(600);

            return new ResponseChatMessageJson
            {
                Reply = BuildSimulatedReply(request.Message)
            };
        }

        private static void Validate(RequestChatMessageJson request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                throw new ErrorOnValidationException(["A mensagem não pode ser vazia."]);
            }
        }

        private static string BuildSimulatedReply(string message)
        {
            return $"""
                [Resposta simulada — Etapa 3]
                Recebi sua pergunta: "{message.Trim()}"

                Na Etapa 4 esta resposta virá do modelo de IA.
                Na Etapa 7, se a pergunta exigir dados, a IA poderá usar ferramentas MCP.
                """;
        }
    }
}
