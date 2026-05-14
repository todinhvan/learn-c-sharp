using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreConcept.JwtAuthDemo.Controllers
{
    public class PolicyController : Controller
    {
        [HttpGet]
        [Authorize(Policy = "Policy1")]
        public IActionResult Hello1()
        {
            return Content("Hello1 authorize with Policy1");
        }

        [HttpGet]
        [Authorize(Policy = "Policy1")]
        [Authorize(Policy = "Policy2")]
        public IActionResult Hello2()
        {
            return Content("Hello1 authorize with Policy1 and Policy2");
        }
    }
}
