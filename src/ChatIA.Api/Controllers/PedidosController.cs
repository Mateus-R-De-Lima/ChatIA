using ChatIA.Application.UseCases.Pedidos.GetByShortReference;
using ChatIA.Application.UseCases.Pedidos.GetCount;
using ChatIA.Application.UseCases.Pedidos.GetLatest;
using ChatIA.Application.UseCases.Pedidos.GetMonthlyRevenue;
using ChatIA.Communication.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ChatIA.Api.Controllers
{
    /// <summary>
    /// Endpoints de leitura de pedidos — úteis para testar o Mongo antes do MCP.
    /// Sem regra de negócio: só orquestra use cases (padrão CashFlow).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        [HttpGet("count")]
        [ProducesResponseType(typeof(ResponsePedidosCountJson), StatusCodes.Status200OK)]
        public async Task<IActionResult> Count(
            [FromServices] IGetPedidosCountUseCase useCase,
            CancellationToken cancellationToken)
        {
            var response = await useCase.Execute(cancellationToken);
            return Ok(response);
        }

        [HttpGet("latest")]
        [ProducesResponseType(typeof(ResponseLatestPedidosJson), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Latest(
            [FromServices] IGetLatestPedidosUseCase useCase,
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            var response = await useCase.Execute(limit, cancellationToken);
            return Ok(response);
        }

        [HttpGet("revenue")]
        [ProducesResponseType(typeof(ResponseMonthlyRevenueJson), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Revenue(
            [FromServices] IGetMonthlyRevenueUseCase useCase,
            [FromQuery] int year,
            [FromQuery] int month,
            CancellationToken cancellationToken = default)
        {
            var response = await useCase.Execute(year, month, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{shortReference:int}")]
        [ProducesResponseType(typeof(ResponsePedidoDetailJson), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ByShortReference(
            [FromServices] IGetPedidoByShortReferenceUseCase useCase,
            [FromRoute] int shortReference,
            CancellationToken cancellationToken = default)
        {
            var response = await useCase.Execute(shortReference, cancellationToken);
            return response is null ? NotFound() : Ok(response);
        }
    }
}
