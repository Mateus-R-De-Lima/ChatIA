using ChatIA.Application.UseCases.Chat.SendMessage;
using ChatIA.Communication.Requests;
using ChatIA.Communication.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ChatIA.Api.Controllers
{
    /// <summary>
    /// Entrada HTTP do chat. Sem regra de negócio — só orquestra o use case (padrão CashFlow).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(ResponseChatMessageJson), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Send(
            [FromServices] ISendChatMessageUseCase useCase,
            [FromBody] RequestChatMessageJson request)
        {
            var response = await useCase.Execute(request);
            return Ok(response);
        }
    }
}
