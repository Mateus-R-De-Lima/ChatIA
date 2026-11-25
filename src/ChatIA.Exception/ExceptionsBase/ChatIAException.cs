namespace ChatIA.Exception.ExceptionsBase
{
    /// <summary>
    /// Exceção base do projeto (mesmo padrão do CashFlowException).
    /// A Api converte essas exceções em respostas HTTP tipadas via filtro.
    /// </summary>
    public abstract class ChatIAException : SystemException
    {
        protected ChatIAException(string message) : base(message)
        {
        }

        public abstract int StatusCode { get; }

        public abstract List<string> GetErrors();
    }
}
