using ChatIA.Domain.Entities;

namespace ChatIA.Domain.Repositories.Pedidos
{
    /// <summary>
    /// Contrato de leitura de pedidos (padrão IExpensesReadOnlyRepository do CashFlow).
    /// Somente operações de leitura nesta etapa.
    /// </summary>
    public interface IPedidoReadOnlyRepository
    {
        Task<long> CountAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Pedido>> GetLatestAsync(
            int limit = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Soma o campo total dos pedidos cujo createdAt está no mês/ano informados.
        /// </summary>
        Task<decimal> GetRevenueByMonthAsync(
            int year,
            int month,
            CancellationToken cancellationToken = default);
    }
}
