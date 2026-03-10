using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SoulNotes.Models;
using SoulNotes.Services;

namespace SoulNotes.Controllers
{
    public class StatisticsController : Controller
    {
        [HttpGet]
        public IActionResult ShowStatistics()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var stats = StatisticsService.GetStatistics(userId.Value);
            var model = new StatisticsViewModel { Stats = stats };

            return View(model);
        }
    }
}