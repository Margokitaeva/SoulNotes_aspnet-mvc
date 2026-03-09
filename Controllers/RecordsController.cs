using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SoulNotes.Models;
using SoulNotes.Services;

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
            model.AllRecords = RecordService.GetAllRecords(userId.Value);
            var allEmotions = EmotionService.GetAllEmotions(userId.Value);
            var allTags = TagService.GetAllTags(userId.Value);
            model.EmotionMap = allEmotions.ToDictionary(e => e.EmotionId);
            model.TagMap = allTags.ToDictionary(e => e.TagId);

            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteRecord(int moodEntryId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            RecordService.DeleteRecord(moodEntryId, userId.Value);
            return RedirectToAction("ShowRecords");
        }
    }
}