using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SoulNotes.Models;

namespace SoulNotes.Controllers
{
    public class RecordsController : Controller
    {
        [HttpGet]
        public IActionResult ShowRecords()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var model = new Records();
            model.AllRecords = DataBaseService.GetAllRecords(userId.Value);
            var allEmotions = DataBaseService.GetAllEmotions(userId.Value);
            var allTags = DataBaseService.GetAllTags(userId.Value);
            model.EmotionMap = allEmotions.ToDictionary(e => e.EmotionId);
            model.TagMap = allTags.ToDictionary(e => e.TagId);
            
            // var allEntries = DataBaseService.GetAllMoodEntriesDebug();
            // ViewData["Debug"] = allEntries;
            return View(model);
        }

        // [HttpPost]
    }
}