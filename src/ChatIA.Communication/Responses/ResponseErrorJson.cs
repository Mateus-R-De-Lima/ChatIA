namespace ChatIA.Communication.Responses
{
    /// <summary>
    /// Contrato padrão de erro da Api (espelha ResponseErrorJson do CashFlow).
    /// </summary>
    public class ResponseErrorJson
    {
        public List<string> ErrorMessages { get; set; }

        public ResponseErrorJson(string errorMessage)
        {
            ErrorMessages = [errorMessage];
        }

        public ResponseErrorJson(List<string> errorMessages)
        {
            ErrorMessages = errorMessages;
        }
    }
}
