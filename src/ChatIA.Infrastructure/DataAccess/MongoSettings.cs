namespace ChatIA.Infrastructure.DataAccess
{
    /// <summary>
    /// Configuração do MongoDB (seção Settings:Mongo + ConnectionStrings:MongoDb).
    /// </summary>
    public class MongoSettings
    {
        public const string SectionName = "Settings:Mongo";

        public string DatabaseName { get; set; } = "homologacao";
        public string PedidosCollectionName { get; set; } = "Pedidos";
    }
}
