using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SoulNotes.Models;

namespace SoulNotes.Controllers
{
    public class EmotionsTagsPageController : Controller
    {
        [HttpGet]
        public IActionResult EmotionsTags()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var model = new NewEmotionsTagsViewModel
            {
                Emotions = DataBaseService.GetAllEmotions(userId.Value),
                Tags = DataBaseService.GetAllTags(userId.Value)
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult EmotionsTags(EmotionsTagsEditForm form, string? ActionType)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (ActionType == "emotion_add" && !string.IsNullOrWhiteSpace(form.CustomEmotionName))
            {
                DataBaseService.AddEmotion(form.CustomEmotionName, form.CustomEmotionColor ?? "#cccccc", userId.Value);
                return RedirectToAction("EmotionsTags");
            }
            if (ActionType == "tag_add" && !string.IsNullOrWhiteSpace(form.CustomTagName))
            {
                DataBaseService.AddTag(form.CustomTagName, userId.Value);
                return RedirectToAction("EmotionsTags");
            }

            if (ActionType == "emotion_delete")
            {
                foreach (var emotionId in form.EmotionIdsToDelete)
                    DataBaseService.DeleteEmotion(emotionId, userId.Value);
            }

            if (ActionType == "tag_delete")
            {
                foreach (var tagId in form.TagIdsToDelete)
                    DataBaseService.DeleteTag(tagId, userId.Value);
            }

            return RedirectToAction("EmotionsTags");
            
        }
    }
}