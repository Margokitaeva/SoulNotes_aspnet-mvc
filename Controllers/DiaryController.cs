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
        public IActionResult DiaryMain(int? formId = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // int formId = -1;

            var model = new NewRecordViewModel
            {
                Emotions = DataBaseService.GetAllEmotions(userId.Value),
                Tags = DataBaseService.GetAllTags(userId.Value)
            };
            if (formId.HasValue)
            {
                RecordFormModel form = DataBaseService.GetFormData(formId.Value, userId.Value);
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

            // _logger.LogInformation("Received POST to DiaryMain. Title: {Title}, PrimaryEmotion: {Emotion}, Tags: {TagCount}",
            //     form.Title, form.PrimaryEmotionId, form.SelectedTagsIds?.Count ?? 0);

            if (addType == "emotion" && !string.IsNullOrWhiteSpace(form.CustomEmotionName))
            {
                DataBaseService.AddEmotion(form.CustomEmotionName, form.CustomEmotionColor ?? "#cccccc", userId.Value);
                model.Emotions = DataBaseService.GetAllEmotions(userId.Value);
                model.Tags = DataBaseService.GetAllTags(userId.Value);
                return View(model);
            }

            if (addType == "tag" && !string.IsNullOrWhiteSpace(form.CustomTagName))
            {
                DataBaseService.AddTag(form.CustomTagName, userId.Value);
                model.Emotions = DataBaseService.GetAllEmotions(userId.Value);
                model.Tags = DataBaseService.GetAllTags(userId.Value);
                return View(model);
            }

            if (!TryValidateModel(form, nameof(model.Form)))
            {
                foreach (var entry in ModelState)
                {
                    Console.WriteLine($"Model error for {entry.Key}: {string.Join(", ", entry.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                ViewBag.TitleError = ModelState["Form.Title"]?.Errors.FirstOrDefault()?.ErrorMessage;
                // ViewBag.DescriptionError = ModelState["Form.Description"]?.Errors.FirstOrDefault()?.ErrorMessage;
                ViewBag.PrimaryEmotionError = ModelState["Form.PrimaryEmotionId"]?.Errors.FirstOrDefault()?.ErrorMessage;
                // ViewBag.DateError = ModelState["Form.RecordDate"]?.Errors.FirstOrDefault()?.ErrorMessage;

                // var model1 = new NewRecordViewModel
                // {
                //     Form = form,
                //     Emotions = DataBaseService.GetAllEmotions(userId.Value),
                //     Tags = DataBaseService.GetAllTags(userId.Value)
                // };
                model.Emotions = DataBaseService.GetAllEmotions(userId.Value);
                model.Tags = DataBaseService.GetAllTags(userId.Value);
                // верни страницу с ошибками
                return View(model);
            }

            long moodEntryId = DataBaseService.AddMoodEntry(
                form.Title,
                form.Description,
                form.PrimaryEmotionId.Value,
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