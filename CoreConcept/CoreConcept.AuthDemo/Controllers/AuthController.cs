using CoreConcept.AuthDemo.Entities;
using CoreConcept.AuthDemo.Models;
using CoreConcept.AuthDemo.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoreConcept.AuthDemo.Controllers
{
    public class AuthController(IUserRepository userRepository) : Controller
    {
        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!IsValidUserName(model.UserName))
            {
                ModelState.AddModelError("UserName", "UserName is invalid");
                return View(model);
            }

            var user = await userRepository.FindByUserNameAsync(model.UserName);
            if (user == null)
            {
                ModelState.AddModelError("UserName", "User not found");
                return View(model);
            }

            string scheme = CookieAuthenticationDefaults.AuthenticationScheme;
            List<Claim> claims = new List<Claim>()
            {
                new(ClaimTypes.Sid, user.UserName),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.MobilePhone, user.PhoneNumber)
            };
            foreach (var role in user.Roles)
            {
                claims.Add(new(ClaimTypes.Role, role));
            }

            ClaimsIdentity identity = new ClaimsIdentity(claims, scheme);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(scheme, principal);

            return RedirectToAction("Index", "Profile");
        }

        [HttpGet("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!IsValidUserName(model.UserName))
            {
                ModelState.AddModelError("UserName", "UserName is invalid");
                return View(model);
            }

            User user = new()
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Password = model.Password,
                Roles = ["User"]
            };
            await userRepository.SaveAsync(user);
            return RedirectToAction("Login");
        }
        private bool IsValidUserName(string userName)
        {
            return !string.IsNullOrEmpty(userName.Trim())
                && userName.All(c => char.IsAsciiLetterOrDigit(c) || c == '-');
        }
    }
}
