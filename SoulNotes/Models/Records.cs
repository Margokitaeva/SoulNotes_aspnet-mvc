namespace SoulNotes.Models
{
    public class Records
    {
        public List<RecordFormModel> AllRecords { get; set; } = new();
        public Dictionary<int, Emotion> EmotionMap { get; set; } = new();
        public Dictionary<int, Tag> TagMap { get; set; } = new();
    }
}