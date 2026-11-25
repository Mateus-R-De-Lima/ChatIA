using ChatIA.Application.UseCases.Pedidos.GetMonthlyRevenue;
using ChatIA.Exception.ExceptionsBase;
using CommonTestUtilities.Repositories;

namespace UseCase.Tests.Pedidos.GetMonthlyRevenue
{
    public class GetMonthlyRevenueUseCaseTest
    {
        [Fact]
        public async Task Success()
        {
            var useCase = new GetMonthlyRevenueUseCase(
                new PedidoReadOnlyRepositoryBuilder().WithRevenue(150.75m).Build());

            var result = await useCase.Execute(2025, 9);

            Assert.Equal(2025, result.Year);
            Assert.Equal(9, result.Month);
            Assert.Equal(150.75m, result.Revenue);
        }

        [Fact]
        public async Task Invalid_Month_Should_Throw()
        {
            var useCase = new GetMonthlyRevenueUseCase(
                new PedidoReadOnlyRepositoryBuilder().WithRevenue(0).Build());

            await Assert.ThrowsAsync<ErrorOnValidationException>(() => useCase.Execute(2025, 13));
        }
    }
}
