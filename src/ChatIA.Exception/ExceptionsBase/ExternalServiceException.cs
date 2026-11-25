using System.Net;

namespace ChatIA.Exception.ExceptionsBase
{
    /// <summary>
    /// Falha ao comunicar com serviço externo (LLM, MongoDB, MCP, etc.).
    /// </summary>
    public class ExternalServiceException : ChatIAException
    {
        public override int StatusCode => (int)HttpStatusCode.BadGateway;

        public ExternalServiceException(string message) : base(message)
        {
        }

        public override List<string> GetErrors() => [Message];
    }
}
