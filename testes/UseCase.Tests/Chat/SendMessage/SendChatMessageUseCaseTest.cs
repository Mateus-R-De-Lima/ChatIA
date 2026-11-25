using ChatIA.Application.UseCases.Chat.SendMessage;
using ChatIA.Communication.Requests;
using ChatIA.Exception.ExceptionsBase;
using CommonTestUtilities.Services;

namespace UseCase.Tests.Chat.SendMessage
{
    public class SendChatMessageUseCaseTest
    {
        [Fact]
        public async Task Success_Should_Return_Ai_Reply()
        {
            var useCase = CreateUseCase(reply: "C# é uma linguagem orientada a objetos.");
            var request = new RequestChatMessageJson { Message = "O que é C#?" };

            var result = await useCase.Execute(request);

            Assert.Equal("C# é uma linguagem orientada a objetos.", result.Reply);
        }

        [Fact]
        public async Task Empty_Message_Should_Throw_Validation_Exception()
        {
            var useCase = CreateUseCase();
            var request = new RequestChatMessageJson { Message = "   " };

            await Assert.ThrowsAsync<ErrorOnValidationException>(() => useCase.Execute(request));
        }

        private static SendChatMessageUseCase CreateUseCase(string reply = "ok")
        {
            var chatAiService = new ChatAiServiceBuilder()
                .WithReply(reply)
                .Build();

            return new SendChatMessageUseCase(chatAiService);
        }
    }
}
