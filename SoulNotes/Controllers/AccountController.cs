using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SoulNotes.Services;

namespace SoulNotes.Controllers {
    public class AccountController : Controller
    {

        string databaseLogin = "Playlist";
        string databasePassword = "Mercy";
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string provided_login, string provided_password)
        {
            if (string.IsNullOrEmpty(provided_login) || string.IsNullOrEmpty(provided_password))
            {
                ViewBag.Error = "Please provide both password and login";
                return View();
            }
            if (UserService.ValidateLogin(provided_login, provided_password))
            {
                var userId = UserService.GetUserId(provided_login);
                if (userId != null)
                {
                    HttpContext.Session.SetInt32("UserId", userId.Value);
                    return RedirectToAction("DiaryMain", "Diary");
                }
            }

            ViewBag.Error = "Wrong login or password";
            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}