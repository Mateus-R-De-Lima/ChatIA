using ChatIA.Communication.Responses;

namespace ChatIA.Application.UseCases.Pedidos.GetLatestDetail
{
    public interface IGetLatestPedidoDetailUseCase
    {
        Task<ResponsePedidoDetailJson?> Execute(CancellationToken cancellationToken = default);
    }
}
