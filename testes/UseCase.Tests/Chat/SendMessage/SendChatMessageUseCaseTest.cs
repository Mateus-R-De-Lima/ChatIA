using ChatIA.Application.UseCases.Chat.SendMessage;
using ChatIA.Communication.Requests;
using ChatIA.Exception.ExceptionsBase;

namespace UseCase.Tests.Chat.SendMessage
{
    public class SendChatMessageUseCaseTest
    {
        [Fact]
        public async Task Success_Should_Return_Simulated_Reply()
        {
            var useCase = new SendChatMessageUseCase();
            var request = new RequestChatMessageJson { Message = "O que é C#?" };

            var result = await useCase.Execute(request);

            Assert.False(string.IsNullOrWhiteSpace(result.Reply));
            Assert.Contains("O que é C#?", result.Reply);
            Assert.Contains("simulada", result.Reply, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Empty_Message_Should_Throw_Validation_Exception()
        {
            var useCase = new SendChatMessageUseCase();
            var request = new RequestChatMessageJson { Message = "   " };

            await Assert.ThrowsAsync<ErrorOnValidationException>(() => useCase.Execute(request));
        }
    }
}
