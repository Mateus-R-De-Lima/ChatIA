using ChatIA.Domain.Entities;
using ChatIA.Domain.Repositories.Pedidos;
using Moq;

namespace CommonTestUtilities.Repositories
{
    public class PedidoReadOnlyRepositoryBuilder
    {
        private readonly Mock<IPedidoReadOnlyRepository> _repository = new();

        public PedidoReadOnlyRepositoryBuilder WithCount(long count)
        {
            _repository
                .Setup(repository => repository.CountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(count);

            return this;
        }

        public PedidoReadOnlyRepositoryBuilder WithLatest(IReadOnlyList<Pedido> pedidos)
        {
            _repository
                .Setup(repository => repository.GetLatestAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(pedidos);

            return this;
        }

        public PedidoReadOnlyRepositoryBuilder WithRevenue(decimal revenue)
        {
            _repository
                .Setup(repository => repository.GetRevenueByMonthAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(revenue);

            return this;
        }

        public IPedidoReadOnlyRepository Build() => _repository.Object;
    }
}
