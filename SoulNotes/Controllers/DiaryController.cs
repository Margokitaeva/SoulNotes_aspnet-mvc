using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SoulNotes.Models;
using SoulNotes.Services;

namespace SoulNotes.Controllers
{
    public class DiaryController : Controller
    {

        [HttpGet]
        public IActionResult DiaryMain(int? formId = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var model = new NewRecordViewModel
            {
                Emotions = EmotionService.GetAllEmotions(userId.Value),
                Tags = TagService.GetAllTags(userId.Value)
            };
            if (formId.HasValue)
            {
                RecordFormModel form = RecordService.GetFormData(formId.Value, userId.Value);
                if (form != null)
                    model.Form = form;
            }
            

            return View(model);
        }

        [HttpPost]
        public IActionResult DiaryMain(NewRecordViewModel model, string? addType)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var form = model.Form;
    
            if (addType == "emotion" && !string.IsNullOrWhiteSpace(form.CustomEmotionName))
            {
                EmotionService.AddEmotion(form.CustomEmotionName, form.CustomEmotionColor ?? "#cccccc", userId.Value);
                model.Emotions = EmotionService.GetAllEmotions(userId.Value);
                model.Tags = TagService.GetAllTags(userId.Value);
                return View(model);
            }

            if (addType == "tag" && !string.IsNullOrWhiteSpace(form.CustomTagName))
            {
                TagService.AddTag(form.CustomTagName, userId.Value);
                model.Emotions = EmotionService.GetAllEmotions(userId.Value);
                model.Tags = TagService.GetAllTags(userId.Value);
                return View(model);
            }

            if (!TryValidateModel(form, nameof(model.Form)))
            {
                foreach (var entry in ModelState)
                {
                    Console.WriteLine($"Model error for {entry.Key}: {string.Join(", ", entry.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                ViewBag.TitleError = ModelState["Form.Title"]?.Errors.FirstOrDefault()?.ErrorMessage;
                ViewBag.PrimaryEmotionError = ModelState["Form.PrimaryEmotionId"]?.Errors.FirstOrDefault()?.ErrorMessage;

                model.Emotions = EmotionService.GetAllEmotions(userId.Value);
                model.Tags = TagService.GetAllTags(userId.Value);
                return View(model);
            }

            long moodEntryId = RecordService.AddMoodEntry(
                form.Title,
                form.Description,
                form.PrimaryEmotionId.Value,
                userId.Value,
                form.RecordDate
            );

            foreach (var emotionId in form.SelectedEmotionsIds.Distinct())
            {
                RecordService.AddEmotionToEntry(moodEntryId, emotionId);
            }

            foreach (var tagId in form.SelectedTagsIds.Distinct())
            {
                RecordService.AddTagToEntry(moodEntryId, tagId);
            }

            return RedirectToAction("DiaryMain");
        }


    }
}