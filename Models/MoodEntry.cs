public class MoodEntry
{
    public long MoodEntryId { get; set; }
    public string Title { get; set; } = "";
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = "";
    public int PrimaryEmotionId { get; set; }

    public Emotion PrimaryEmotion { get; set; } = null!;
    public List<EmotionEntry> EmotionEntries { get; set; } = new();
    public List<TagEntry> TagEntries { get; set; } = new();
}