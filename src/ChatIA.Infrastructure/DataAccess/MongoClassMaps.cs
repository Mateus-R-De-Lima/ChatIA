using ChatIA.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace ChatIA.Infrastructure.DataAccess
{
    /// <summary>
    /// Mapeia entidades do Domain para os nomes reais dos campos no BSON.
    /// Mantém o Domain livre de atributos do MongoDB (padrão Clean Architecture).
    /// </summary>
    public static class MongoClassMaps
    {
        private static bool _registered;

        public static void Register()
        {
            if (_registered)
            {
                return;
            }

            // Decimal do C# ↔ NumberDecimal / Decimal128 do MongoDB
            BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));

            if (!BsonClassMap.IsClassMapRegistered(typeof(Pedido)))
            {
                BsonClassMap.RegisterClassMap<Pedido>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(p => p.Id)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    cm.MapProperty(p => p.EmpresaId).SetElementName("empresaId");
                    cm.MapProperty(p => p.PedidoId).SetElementName("pedido_id");
                    cm.MapProperty(p => p.CreatedAt).SetElementName("createdAt");
                    cm.MapProperty(p => p.Customer).SetElementName("customer");
                    cm.MapProperty(p => p.Items).SetElementName("items");
                    cm.MapProperty(p => p.Payments).SetElementName("payments");
                    cm.MapProperty(p => p.Total).SetElementName("total");
                    cm.MapProperty(p => p.Type).SetElementName("type");
                    cm.MapProperty(p => p.SalesChannel).SetElementName("salesChannel");
                    cm.MapProperty(p => p.ShortReference).SetElementName("shortReference");
                    cm.SetIgnoreExtraElements(true);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(Customer)))
            {
                BsonClassMap.RegisterClassMap<Customer>(cm =>
                {
                    cm.AutoMap();
                    cm.MapProperty(c => c.Id).SetElementName("_id");
                    cm.MapProperty(c => c.Name).SetElementName("Name");
                    cm.MapProperty(c => c.Phone).SetElementName("Phone");
                    cm.MapProperty(c => c.TaxPayerIdentificationNumber)
                        .SetElementName("TaxPayerIdentificationNumber");
                    cm.SetIgnoreExtraElements(true);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(PedidoItem)))
            {
                BsonClassMap.RegisterClassMap<PedidoItem>(cm =>
                {
                    cm.AutoMap();
                    cm.MapProperty(i => i.Name).SetElementName("Name");
                    cm.MapProperty(i => i.Quantity).SetElementName("Quantity");
                    cm.MapProperty(i => i.Price).SetElementName("Price");
                    cm.MapProperty(i => i.Total).SetElementName("Total");
                    cm.MapProperty(i => i.Observation).SetElementName("Observation");
                    cm.MapProperty(i => i.InternalId).SetElementName("InternalId");
                    cm.SetIgnoreExtraElements(true);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(PedidoPayment)))
            {
                BsonClassMap.RegisterClassMap<PedidoPayment>(cm =>
                {
                    cm.AutoMap();
                    cm.MapProperty(p => p.Name).SetElementName("Name");
                    cm.MapProperty(p => p.Code).SetElementName("Code");
                    cm.MapProperty(p => p.Value).SetElementName("Value");
                    cm.MapProperty(p => p.Prepaid).SetElementName("Prepaid");
                    cm.SetIgnoreExtraElements(true);
                });
            }

            _registered = true;
        }
    }
}
