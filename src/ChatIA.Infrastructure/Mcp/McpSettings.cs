namespace ChatIA.Infrastructure.Mcp
{
    /// <summary>
    /// Como a Api (cliente MCP) sobe o ChatIA.McpServer via stdio.
    /// </summary>
    public class McpSettings
    {
        public const string SectionName = "Settings:Mcp";

        /// <summary>
        /// true = lista tools MCP e permite tool calling no chat.
        /// </summary>
        public bool Enabled { get; set; } = true;

        public string Command { get; set; } = "dotnet";

        /// <summary>
        /// Caminho relativo (a partir do content root da Api) ou absoluto do .csproj do McpServer.
        /// Ex.: ../ChatIA.McpServer/ChatIA.McpServer.csproj
        /// </summary>
        public string ServerProjectPath { get; set; } = "../ChatIA.McpServer/ChatIA.McpServer.csproj";
    }
}
