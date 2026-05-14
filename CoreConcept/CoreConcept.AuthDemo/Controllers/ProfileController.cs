using CoreConcept.AuthDemo.Models;
using CoreConcept.AuthDemo.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreConcept.AuthDemo.Controllers
{
    public class ProfileController(IUserRepository userRepository) : Controller
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await userRepository.FindByUserNameAsync(User.Identity!.Name!);
            if (user == null)
            {
                return NotFound();
            }

            return View(new ProfileModel()
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = user.Roles,
            });
        }
    }
}
