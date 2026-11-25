using ChatIA.Communication.Responses;
using ChatIA.Domain.Repositories.Pedidos;

namespace ChatIA.Application.UseCases.Pedidos.GetCount
{
    public class GetPedidosCountUseCase(IPedidoReadOnlyRepository repository) : IGetPedidosCountUseCase
    {
        public async Task<ResponsePedidosCountJson> Execute(CancellationToken cancellationToken = default)
        {
            var count = await repository.CountAsync(cancellationToken);

            return new ResponsePedidosCountJson { Count = count };
        }
    }
}
