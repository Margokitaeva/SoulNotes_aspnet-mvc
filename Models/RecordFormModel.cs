using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SoulNotes.Models
{
    public class RecordFormModel
    {
        public int MoodEntryId { get; set; }
        [Required(ErrorMessage = "Write a title please")]
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        [Required(ErrorMessage = "Select a primary emotion please")]
        public int PrimaryEmotionId { get; set; }
        public List<int> SelectedEmotionsIds { get; set; } = new();
        public List<int> SelectedTagsIds { get; set; } = new();

        public string? CustomEmotionName { get; set; }
        public string? CustomEmotionColor { get; set; }
        public string? CustomTagName { get; set; }
        [Required(ErrorMessage = "Select a date please")]
        public DateTime RecordDate { get; set; }

    }
}