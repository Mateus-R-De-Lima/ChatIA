namespace ChatIA.Infrastructure.Ai
{
    /// <summary>
    /// Configurações do provedor de IA (seção Settings:Ai).
    /// Prefira não versionar a ApiKey real (use appsettings.Development.json local ou User Secrets).
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
            "Para perguntas conceituais (ex.: o que é C#?, Clean Architecture), responda com conhecimento. " +
            "Nunca invente dados de pedidos, números ou faturamento. " +
            "Nunca escreva JSON nem simule function calls.";
    }
}
