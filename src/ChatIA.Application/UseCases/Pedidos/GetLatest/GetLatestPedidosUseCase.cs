using ChatIA.Communication.Responses;
using ChatIA.Domain.Repositories.Pedidos;
using ChatIA.Exception.ExceptionsBase;

namespace ChatIA.Application.UseCases.Pedidos.GetLatest
{
    public class GetLatestPedidosUseCase(IPedidoReadOnlyRepository repository) : IGetLatestPedidosUseCase
    {
        public async Task<ResponseLatestPedidosJson> Execute(
            int limit = 10,
            CancellationToken cancellationToken = default)
        {
            if (limit is < 1 or > 50)
            {
                throw new ErrorOnValidationException(["O limite deve estar entre 1 e 50."]);
            }

            var pedidos = await repository.GetLatestAsync(limit, cancellationToken);

            return new ResponseLatestPedidosJson
            {
                Pedidos = pedidos.Select(pedido => new ResponsePedidoShortJson
                {
                    Id = pedido.Id,
                    ShortReference = pedido.ShortReference.ToString(),
                    CustomerName = pedido.Customer?.Name ?? string.Empty,
                    Total = pedido.Total,
                    Type = pedido.Type,
                    SalesChannel = pedido.SalesChannel,
                    CreatedAt = pedido.CreatedAt
                }).ToList()
            };
        }
    }
}
