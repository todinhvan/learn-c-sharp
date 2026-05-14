using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreConcept.JwtAuthDemo.Controllers
{
    public class RoleController : Controller
    {
        [HttpGet]
        public IActionResult Hello1()
        {
            return Content("Hello1 must have Jwt Token");
        }

        [HttpGet]
        [Authorize(Roles = "Role1,Role2")]
        public IActionResult Hello2()
        {
            return Content("Hello2 must have Jwt Token and Payload with Role1 or Role2");
        }

        [HttpGet]
        [Authorize(Roles = "Role1")]
        [Authorize(Roles = "Role2")]
        public IActionResult Hello3()
        {
            return Content("Hello3 must have Jwt Token and Payload with Role1 and Role2");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Hello4()
        {
            return Content("Hello4 not Authorize");
        }
    }
}
