namespace ChatIA.Infrastructure.Ai
{
    /// <summary>
    /// Configurações do provedor de IA (seção Settings:Ai).
    /// ApiKey NÃO deve ir para o repositório — use User Secrets ou variáveis de ambiente.
    /// </summary>
    public class AiSettings
    {
        public const string SectionName = "Settings:Ai";

        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4o-mini";

        /// <summary>
        /// Opcional. Preencha para endpoints compatíveis com OpenAI (ex.: Ollama, Azure-compatible).
        /// Deixe vazio para a API oficial da OpenAI.
        /// </summary>
        public string? Endpoint { get; set; }

        public string SystemPrompt { get; set; } =
            "Você é o assistente do ChatIA, um projeto de aprendizado em C#/.NET. " +
            "Responda em português, de forma clara e didática. " +
            "Quando a pergunta exigir dados de pedidos/faturamento do banco, use as tools disponíveis " +
            "(get_pedidos_count, get_latest_pedidos, get_monthly_revenue). " +
            "Para perguntas conceituais (ex.: o que é C#?), responda diretamente sem tools.";
    }
}
