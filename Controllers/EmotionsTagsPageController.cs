using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SoulNotes.Models;
using SoulNotes.Services;

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
                Emotions = EmotionService.GetAllEmotions(userId.Value),
                Tags = TagService.GetAllTags(userId.Value)
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
                EmotionService.AddEmotion(form.CustomEmotionName, form.CustomEmotionColor ?? "#cccccc", userId.Value);
                return RedirectToAction("EmotionsTags");
            }
            if (ActionType == "tag_add" && !string.IsNullOrWhiteSpace(form.CustomTagName))
            {
                TagService.AddTag(form.CustomTagName, userId.Value);
                return RedirectToAction("EmotionsTags");
            }

            if (ActionType == "emotion_delete")
            {
                foreach (var emotionId in form.EmotionIdsToDelete)
                    EmotionService.DeleteEmotion(emotionId, userId.Value);
            }

            if (ActionType == "tag_delete")
            {
                foreach (var tagId in form.TagIdsToDelete)
                    TagService.DeleteTag(tagId, userId.Value);
            }

            return RedirectToAction("EmotionsTags");
            
        }
    }
}