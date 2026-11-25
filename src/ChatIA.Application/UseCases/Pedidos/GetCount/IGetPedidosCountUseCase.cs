using ChatIA.Communication.Responses;

namespace ChatIA.Application.UseCases.Pedidos.GetCount
{
    public interface IGetPedidosCountUseCase
    {
        Task<ResponsePedidosCountJson> Execute(CancellationToken cancellationToken = default);
    }
}
