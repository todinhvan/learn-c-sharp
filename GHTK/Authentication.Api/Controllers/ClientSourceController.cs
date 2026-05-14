using Authentication.Client;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientSourceController(IClientSourceAuthenticationHandler authenticationHandler) : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> Verify([FromRoute] string id)
        {
            if (await authenticationHandler.AuthenticateAsync(id))
            {
                return Ok();
            }
            return Unauthorized();
        }
    }
}
