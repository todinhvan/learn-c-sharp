using Microsoft.AspNetCore.Mvc;
using MySession.Custom.CustomSession;
using MySession.Custom.Models;
using System.Diagnostics;

namespace MySession.Custom.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            HttpContext.GetCustomSession().SetString("Name", "Van");
            var name = HttpContext.GetCustomSession().GetString("Name");
            return View("Index", name);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> SetCustomSession(string key, string value)
        {
            var session = HttpContext.GetCustomSession();
            session.SetString(key, value);
            await session.CommitAsync();
            return Ok();
        }

        public async Task<IActionResult> GetCustomSession(string key)
        {
            var session = HttpContext.GetCustomSession();
            await session.LoadAsync();
            return Ok(session.GetString(key));
        }
    }
}
