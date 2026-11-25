using ChatIA.Domain.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ChatIA.Infrastructure.DataAccess
{
    /// <summary>
    /// Acesso tipado ao banco MongoDB (equivalente didático ao CashFlowDbContext).
    /// </summary>
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly MongoSettings _settings;

        public MongoDbContext(IMongoClient client, IOptions<MongoSettings> options)
        {
            _settings = options.Value;
            _database = client.GetDatabase(_settings.DatabaseName);
        }

        public IMongoCollection<Pedido> Pedidos =>
            _database.GetCollection<Pedido>(_settings.PedidosCollectionName);
    }
}
