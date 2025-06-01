using System.ComponentModel.DataAnnotations;

namespace SoulNotes.Models
{
    public class EmotionsTagsEditForm
    {
        public string? CustomEmotionName { get; set; }
        public string? CustomEmotionColor { get; set; }
        public string? CustomTagName { get; set; }
        public List<int> EmotionIdsToDelete { get; set; } = new();
        public List<int> TagIdsToDelete { get; set; } = new();
    }
}