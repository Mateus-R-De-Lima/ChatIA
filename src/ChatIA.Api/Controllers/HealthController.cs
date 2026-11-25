using Microsoft.AspNetCore.Mvc;

namespace ChatIA.Api.Controllers
{
    /// <summary>
    /// Endpoint simples para validar que a Api sobe e a DI está registrada.
    /// Será substituído/complementado pelo chat nas próximas etapas.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return Ok(new { status = "healthy", project = "ChatIA" });
        }
    }
}
