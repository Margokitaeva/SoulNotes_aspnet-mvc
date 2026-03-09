using Microsoft.AspNetCore.Mvc;
using SoulNotes.Models;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class ApiController : ControllerBase
{
    [HttpGet("records")]
    public IActionResult GetAllRecords(string login, string token)
    {
        if (!AuthService.Validate(login, token))
            return Unauthorized("Invalid token");

        int? userId = DataBaseService.GetUserIdByLogin(login);
        if (userId == null)
            return NotFound("User not found");

        var records = DataBaseService.GetAllRecords(userId.Value);
        return Ok(records);
    }

    [HttpPost("records")]
    public IActionResult AddRecord(string login, string token, [FromBody] RecordFormModel model)
    {
        if (!AuthService.Validate(login, token))
            return Unauthorized("Invalid token");

        int? userId = DataBaseService.GetUserIdByLogin(login);
        if (userId == null)
            return NotFound("User not found");

        long id = DataBaseService.AddMoodEntry(
            model.Title,
            model.Description,
            model.PrimaryEmotionId.Value,
            userId.Value,
            model.RecordDate
        );

        return Ok(new { moodEntryId = id });
    }

    [HttpGet("emotions")]
    public IActionResult GetAllEmotions(string login, string token)
    {
        if (!AuthService.Validate(login, token))
            return Unauthorized("Invalid token");

        int? userId = DataBaseService.GetUserIdByLogin(login);
        if (userId == null)
            return NotFound("User not found");

        var emotions = DataBaseService.GetAllEmotions(userId.Value);
        return Ok(emotions);
    }

    [HttpPost("emotions")]
    public IActionResult AddEmotion(string login, string token, [FromBody] Emotion emotion)
    {
        if (!AuthService.Validate(login, token))
            return Unauthorized("Invalid token");

        int? userId = DataBaseService.GetUserIdByLogin(login);
        if (userId == null)
            return NotFound("User not found");

        DataBaseService.AddEmotion(emotion.Name, emotion.Color, userId.Value);
        return Ok("Emotion added.");
    }

    [HttpDelete("emotions/{id}")]
    public IActionResult DeleteEmotion(string login, string token, int id)
    {
        if (!AuthService.Validate(login, token))
            return Unauthorized("Invalid token");

        int? userId = DataBaseService.GetUserIdByLogin(login);
        if (userId == null)
            return NotFound("User not found");

        DataBaseService.DeleteEmotion(id, userId.Value);
        return Ok("Emotion deleted.");
    }

    [HttpGet("tags")]
    public IActionResult GetAllTags(string login, string token)
    {
        if (!AuthService.Validate(login, token))
            return Unauthorized("Invalid token");

        int? userId = DataBaseService.GetUserIdByLogin(login);
        if (userId == null)
            return NotFound("User not found");

        var tags = DataBaseService.GetAllTags(userId.Value);
        return Ok(tags);
    }

    [HttpPost("tags")]
    public IActionResult AddTag(string login, string token, [FromBody] Tag tag)
    {
        if (!AuthService.Validate(login, token))
            return Unauthorized("Invalid token");

        int? userId = DataBaseService.GetUserIdByLogin(login);
        if (userId == null)
            return NotFound("User not found");

        DataBaseService.AddTag(tag.Name, userId.Value);
        return Ok("Tag added.");
    }

    [HttpDelete("tags/{id}")]
    public IActionResult DeleteTag(string login, string token, int id)
    {
        if (!AuthService.Validate(login, token))
            return Unauthorized("Invalid token");

        int? userId = DataBaseService.GetUserIdByLogin(login);
        if (userId == null)
            return NotFound("User not found");

        DataBaseService.DeleteTag(id, userId.Value);
        return Ok("Tag deleted.");
    }
}