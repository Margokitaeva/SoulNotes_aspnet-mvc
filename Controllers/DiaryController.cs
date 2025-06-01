using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SoulNotes.Models;

namespace SoulNotes.Controllers
{
    public class DiaryController : Controller
    {
        // private readonly ILogger<DiaryController> _logger;

        // public DiaryController(ILogger<DiaryController> logger)
        // {
        //     _logger = logger;
        // }

        [HttpGet]
        public IActionResult DiaryMain()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            int formId = -1;

            var model = new NewRecordViewModel
            {
                Emotions = DataBaseService.GetAllEmotions(userId.Value),
                Tags = DataBaseService.GetAllTags(userId.Value)
            };
            RecordFormModel form = DataBaseService.GetFormData(formId, userId.Value);
            if (form != null)
                model.Form = form;

            return View(model);
        }

        [HttpPost]
        public IActionResult DiaryMain(RecordFormModel form, string? addType)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // _logger.LogInformation("Received POST to DiaryMain. Title: {Title}, PrimaryEmotion: {Emotion}, Tags: {TagCount}",
            //     form.Title, form.PrimaryEmotionId, form.SelectedTagsIds?.Count ?? 0);

            if (addType == "emotion" && !string.IsNullOrWhiteSpace(form.CustomEmotionName))
            {
                DataBaseService.AddEmotion(form.CustomEmotionName, form.CustomEmotionColor ?? "#cccccc", userId.Value);
                return RedirectToAction("DiaryMain");
            }

            if (addType == "tag" && !string.IsNullOrWhiteSpace(form.CustomTagName))
            {
                DataBaseService.AddTag(form.CustomTagName, userId.Value);
                return RedirectToAction("DiaryMain");
            }

            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    Console.WriteLine($"Model error for {entry.Key}: {string.Join(", ", entry.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                var model = new NewRecordViewModel
                {
                    Form = form,
                    Emotions = DataBaseService.GetAllEmotions(userId.Value),
                    Tags = DataBaseService.GetAllTags(userId.Value)
                };
                // верни страницу с ошибками
                return View(model);
            }

            long moodEntryId = DataBaseService.AddMoodEntry(
                form.Title,
                form.Description,
                form.PrimaryEmotionId,
                userId.Value,
                form.RecordDate
            );

            foreach (var emotionId in form.SelectedEmotionsIds.Distinct())
            {
                DataBaseService.AddEmotionToEntry(moodEntryId, emotionId);
            }

            foreach (var tagId in form.SelectedTagsIds.Distinct())
            {
                DataBaseService.AddTagToEntry(moodEntryId, tagId);
            }

            return RedirectToAction("DiaryMain");
        }


    }
}