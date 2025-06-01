using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace SoulNotes.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult UserManagement()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || !DataBaseService.IsUserAdmin(userId.Value))
                return RedirectToAction("Login", "Account");

            var users = DataBaseService.GetAllUsers();
            ViewBag.Users = users;
            return View();
        }

        [HttpPost]
        public IActionResult CreateUser(string login, string password)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || !DataBaseService.IsUserAdmin(userId.Value))
                return RedirectToAction("Login", "Account");

            DataBaseService.CreateUser(login, password);
            return RedirectToAction("UserManagement");
        }
    }
}