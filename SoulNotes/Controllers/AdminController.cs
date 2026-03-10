using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SoulNotes.Services;

namespace SoulNotes.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult UserManagement()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || !UserService.IsUserAdmin(userId.Value))
                return RedirectToAction("Login", "Account");

            var users = UserService.GetAllUsers();
            ViewBag.Users = users;
            return View();
        }

        [HttpPost]
        public IActionResult CreateUser(string login, string password)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || !UserService.IsUserAdmin(userId.Value))
                return RedirectToAction("Login", "Account");

            UserService.CreateUser(login, password);
            return RedirectToAction("UserManagement");
        }
    }
}