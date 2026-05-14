using CoreConcept.MiddlewareDemo.Models;
using CoreConcept.MiddlewareDemo.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CoreConcept.MiddlewareDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientInfoController : ControllerBase
    {
        private readonly IClientInfoRepository _clientInfoRepository;

        public ClientInfoController(IClientInfoRepository clientInfoRepository)
        {
            _clientInfoRepository = clientInfoRepository;
        }

        [HttpGet("{id}")]
        public IActionResult GetClientInfo([FromRoute] int id)
        {
            var clientInfo = _clientInfoRepository.GetClientInfo(id);
            if (clientInfo == null)
            {
                return BadRequest($"Client with id {id} not found.");
            }
            return Ok(clientInfo);
        }

        [HttpGet("api-settings")]
        public IActionResult GetApiSettings()
        {
            var apiSettings = HttpContext.Features.Get<ApiSettings>();
            if (apiSettings == null)
            {
                return BadRequest("API settings not found.");
            }
            return Ok(apiSettings);
        }
    }
}
