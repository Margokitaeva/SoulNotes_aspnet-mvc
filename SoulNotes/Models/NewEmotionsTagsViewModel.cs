namespace SoulNotes.Models
{
    public class NewEmotionsTagsViewModel
    {
        public List<Emotion> Emotions { get; set; } = new();
        public List<Tag> Tags { get; set; } = new();
    }
}