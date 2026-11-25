using ChatIA.Communication.Responses;

namespace ChatIA.Application.UseCases.Pedidos.GetMonthlyRevenue
{
    public interface IGetMonthlyRevenueUseCase
    {
        Task<ResponseMonthlyRevenueJson> Execute(
            int year,
            int month,
            CancellationToken cancellationToken = default);
    }
}
