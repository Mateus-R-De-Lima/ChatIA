using ChatIA.Communication.Responses;
using ChatIA.Domain.Repositories.Pedidos;

namespace ChatIA.Application.UseCases.Pedidos.GetLatestDetail
{
    public class GetLatestPedidoDetailUseCase(IPedidoReadOnlyRepository repository)
        : IGetLatestPedidoDetailUseCase
    {
        public async Task<ResponsePedidoDetailJson?> Execute(
            CancellationToken cancellationToken = default)
        {
            var pedidos = await repository.GetLatestAsync(1, cancellationToken);
            var pedido = pedidos.FirstOrDefault();
            return pedido is null ? null : PedidoDetailMapper.ToDetail(pedido);
        }
    }
}
