using ChatIA.Communication.Requests;
using ChatIA.Communication.Responses;

namespace ChatIA.Application.UseCases.Chat.SendMessage
{
    public interface ISendChatMessageUseCase
    {
        Task<ResponseChatMessageJson> Execute(RequestChatMessageJson request);
    }
}
