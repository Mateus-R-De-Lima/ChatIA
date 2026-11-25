using ChatIA.Communication.Responses;

namespace ChatIA.Application.UseCases.Pedidos.GetLatest
{
    public interface IGetLatestPedidosUseCase
    {
        Task<ResponseLatestPedidosJson> Execute(int limit = 10, CancellationToken cancellationToken = default);
    }
}
