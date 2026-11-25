using Microsoft.Extensions.AI;

namespace ChatIA.Infrastructure.Mcp
{
    /// <summary>
    /// Obtém as tools expostas pelo servidor MCP para o LLM usar.
    /// </summary>
    public interface IMcpToolProvider
    {
        Task<IList<AITool>> GetToolsAsync(CancellationToken cancellationToken = default);
    }
}
