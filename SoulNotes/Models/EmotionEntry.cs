public class EmotionEntry
{
    public int EmotionEntryId { get; set; }
    public long MoodEntryId { get; set; }
    public MoodEntry MoodEntry { get; set; } = null!;
    public int EmotionId { get; set; }
    public Emotion Emotion { get; set; } = null!;
}