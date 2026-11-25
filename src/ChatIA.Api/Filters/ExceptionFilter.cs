using ChatIA.Communication.Responses;
using ChatIA.Exception.ExceptionsBase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ChatIA.Api.Filters
{
    /// <summary>
    /// Filtro global de exceções (equivalente ao ExecptionFilter do CashFlow).
    /// Controllers não tratam erro manualmente: lançam ChatIAException e o filtro responde.
    /// </summary>
    public class ExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is ChatIAException)
            {
                HandleProjectException(context);
            }
            else
            {
                ThrowUnknownError(context);
            }
        }

        private static void HandleProjectException(ExceptionContext context)
        {
            var chatIaException = (ChatIAException)context.Exception;
            var errorResponse = new ResponseErrorJson(chatIaException.GetErrors());

            context.HttpContext.Response.StatusCode = chatIaException.StatusCode;
            context.Result = new ObjectResult(errorResponse);
        }

        private static void ThrowUnknownError(ExceptionContext context)
        {
            var errorResponse = new ResponseErrorJson("Unknown Error");

            context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Result = new ObjectResult(errorResponse);
        }
    }
}
