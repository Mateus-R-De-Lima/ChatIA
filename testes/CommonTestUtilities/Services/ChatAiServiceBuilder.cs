using ChatIA.Domain.Services.Ai;
using Moq;

namespace CommonTestUtilities.Services
{
    public class ChatAiServiceBuilder
    {
        private readonly Mock<IChatAiService> _service = new();

        public ChatAiServiceBuilder()
        {
            _service
                .Setup(service => service.GetReplyAsync(
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyList<ChatTurn>?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("Resposta mockada da IA");
        }

        public ChatAiServiceBuilder WithReply(string reply)
        {
            _service
                .Setup(service => service.GetReplyAsync(
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyList<ChatTurn>?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reply);

            return this;
        }

        public IChatAiService Build() => _service.Object;
    }
}
