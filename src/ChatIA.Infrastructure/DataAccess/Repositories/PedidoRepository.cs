using ChatIA.Domain.Entities;
using ChatIA.Domain.Repositories.Pedidos;
using ChatIA.Exception.ExceptionsBase;
using MongoDB.Driver;

namespace ChatIA.Infrastructure.DataAccess.Repositories
{
    /// <summary>
    /// Implementação MongoDB do repositório de leitura (internal, como no CashFlow).
    /// </summary>
    internal class PedidoRepository(MongoDbContext dbContext) : IPedidoReadOnlyRepository
    {
        private const int MaxLimit = 50;

        public async Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await dbContext.Pedidos.CountDocumentsAsync(
                    FilterDefinition<Pedido>.Empty,
                    cancellationToken: cancellationToken);
            }
            catch (System.Exception ex)
            {
                throw new ExternalServiceException($"Falha ao contar pedidos no MongoDB: {ex.Message}");
            }
        }

        public async Task<IReadOnlyList<Pedido>> GetLatestAsync(
            int limit = 10,
            CancellationToken cancellationToken = default)
        {
            var take = Math.Clamp(limit, 1, MaxLimit);

            try
            {
                return await dbContext.Pedidos
                    .Find(FilterDefinition<Pedido>.Empty)
                    .SortByDescending(pedido => pedido.CreatedAt)
                    .Limit(take)
                    .ToListAsync(cancellationToken);
            }
            catch (System.Exception ex)
            {
                throw new ExternalServiceException($"Falha ao buscar pedidos no MongoDB: {ex.Message}");
            }
        }

        public async Task<decimal> GetRevenueByMonthAsync(
            int year,
            int month,
            CancellationToken cancellationToken = default)
        {
            var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);

            try
            {
                var filter = Builders<Pedido>.Filter.And(
                    Builders<Pedido>.Filter.Gte(pedido => pedido.CreatedAt, start),
                    Builders<Pedido>.Filter.Lt(pedido => pedido.CreatedAt, end));

                var totals = await dbContext.Pedidos
                    .Find(filter)
                    .Project(pedido => pedido.Total)
                    .ToListAsync(cancellationToken);

                return totals.Sum();
            }
            catch (System.Exception ex)
            {
                throw new ExternalServiceException($"Falha ao calcular faturamento no MongoDB: {ex.Message}");
            }
        }
    }
}
