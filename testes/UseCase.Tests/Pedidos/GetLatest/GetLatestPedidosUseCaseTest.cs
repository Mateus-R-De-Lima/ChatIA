using ChatIA.Application.UseCases.Pedidos.GetLatest;
using ChatIA.Domain.Entities;
using ChatIA.Exception.ExceptionsBase;
using CommonTestUtilities.Repositories;

namespace UseCase.Tests.Pedidos.GetLatest
{
    public class GetLatestPedidosUseCaseTest
    {
        [Fact]
        public async Task Success()
        {
            var pedidos = new List<Pedido>
            {
                new()
                {
                    Id = "1",
                    ShortReference = 48,
                    Total = 7.5m,
                    Type = "TAKE",
                    SalesChannel = "anotaai",
                    CreatedAt = DateTime.UtcNow,
                    Customer = new Customer { Name = "mateus lima" }
                }
            };

            var useCase = new GetLatestPedidosUseCase(
                new PedidoReadOnlyRepositoryBuilder().WithLatest(pedidos).Build());

            var result = await useCase.Execute(10);

            Assert.Single(result.Pedidos);
            Assert.Equal("mateus lima", result.Pedidos[0].CustomerName);
            Assert.Equal(7.5m, result.Pedidos[0].Total);
        }

        [Fact]
        public async Task Invalid_Limit_Should_Throw()
        {
            var useCase = new GetLatestPedidosUseCase(
                new PedidoReadOnlyRepositoryBuilder().WithLatest([]).Build());

            await Assert.ThrowsAsync<ErrorOnValidationException>(() => useCase.Execute(0));
        }
    }
}
