using ChatIA.Application.UseCases.Pedidos.GetCount;
using CommonTestUtilities.Repositories;

namespace UseCase.Tests.Pedidos.GetCount
{
    public class GetPedidosCountUseCaseTest
    {
        [Fact]
        public async Task Success()
        {
            var useCase = new GetPedidosCountUseCase(
                new PedidoReadOnlyRepositoryBuilder().WithCount(42).Build());

            var result = await useCase.Execute();

            Assert.Equal(42, result.Count);
        }
    }
}
