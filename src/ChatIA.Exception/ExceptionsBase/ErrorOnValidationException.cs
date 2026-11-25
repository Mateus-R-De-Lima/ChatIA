using System.Net;

namespace ChatIA.Exception.ExceptionsBase
{
    /// <summary>
    /// Usada quando a validação de entrada falha (FluentValidation nos use cases).
    /// </summary>
    public class ErrorOnValidationException : ChatIAException
    {
        private readonly List<string> _errorMessages;

        public override int StatusCode => (int)HttpStatusCode.BadRequest;

        public ErrorOnValidationException(List<string> errorMessages) : base(string.Empty)
        {
            _errorMessages = errorMessages;
        }

        public override List<string> GetErrors() => _errorMessages;
    }
}
