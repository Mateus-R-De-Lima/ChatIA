using ChatIA.Communication.Responses;
using ChatIA.Domain.Repositories.Pedidos;
using ChatIA.Exception.ExceptionsBase;

namespace ChatIA.Application.UseCases.Pedidos.GetByShortReference
{
    public class GetPedidoByShortReferenceUseCase(IPedidoReadOnlyRepository repository)
        : IGetPedidoByShortReferenceUseCase
    {
        public async Task<ResponsePedidoDetailJson?> Execute(
            int shortReference,
            CancellationToken cancellationToken = default)
        {
            if (shortReference < 1)
            {
                throw new ErrorOnValidationException(
                    ["A referência do pedido deve ser um número positivo."]);
            }

            var pedido = await repository.GetByShortReferenceAsync(shortReference, cancellationToken);
            return pedido is null ? null : PedidoDetailMapper.ToDetail(pedido);
        }
    }
}
