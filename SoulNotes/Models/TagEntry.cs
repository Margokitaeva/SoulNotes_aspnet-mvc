public class TagEntry
{
    public int TagEntryId { get; set; }
    public long MoodEntryId { get; set; }
    public MoodEntry MoodEntry { get; set; } = null!;
    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}