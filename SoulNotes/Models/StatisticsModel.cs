public class StatisticsModel
{
    public int TotalRecords { get; set; }
    public int RecordsToday { get; set; }
    public int RecordsThisWeek { get; set; }
    public int RecordsThisMonth { get; set; }

    public Dictionary<string, int> EmotionCounts { get; set; } = new();
    public string TopEmotionWeek { get; set; }
    public string TopEmotionMonth { get; set; }

    public List<Emotion> TopEmotions { get; set; } = new();
    public List<Tag> TopTags { get; set; } = new();
    public Dictionary<Emotion, List<Tag>> EmotionTagCorrelation { get; set; } = new();

}