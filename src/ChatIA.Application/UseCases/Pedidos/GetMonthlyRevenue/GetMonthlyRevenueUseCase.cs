using ChatIA.Communication.Responses;
using ChatIA.Domain.Repositories.Pedidos;
using ChatIA.Exception.ExceptionsBase;

namespace ChatIA.Application.UseCases.Pedidos.GetMonthlyRevenue
{
    public class GetMonthlyRevenueUseCase(IPedidoReadOnlyRepository repository) : IGetMonthlyRevenueUseCase
    {
        public async Task<ResponseMonthlyRevenueJson> Execute(
            int year,
            int month,
            CancellationToken cancellationToken = default)
        {
            if (year < 2000 || year > DateTime.UtcNow.Year + 1)
            {
                throw new ErrorOnValidationException(["Ano inválido."]);
            }

            if (month is < 1 or > 12)
            {
                throw new ErrorOnValidationException(["Mês inválido. Use um valor de 1 a 12."]);
            }

            var revenue = await repository.GetRevenueByMonthAsync(year, month, cancellationToken);

            return new ResponseMonthlyRevenueJson
            {
                Year = year,
                Month = month,
                Revenue = revenue
            };
        }
    }
}
