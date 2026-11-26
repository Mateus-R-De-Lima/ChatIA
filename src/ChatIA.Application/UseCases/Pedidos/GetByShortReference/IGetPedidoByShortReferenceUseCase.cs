using ChatIA.Communication.Responses;

namespace ChatIA.Application.UseCases.Pedidos.GetByShortReference
{
    public interface IGetPedidoByShortReferenceUseCase
    {
        Task<ResponsePedidoDetailJson?> Execute(
            int shortReference,
            CancellationToken cancellationToken = default);
    }
}
