using System.Collections.Generic;

namespace SoulNotes.Models
{
    public class NewRecordViewModel
    {
        public RecordFormModel Form { get; set; } = new();
        public List<Emotion> Emotions { get; set; } = new();
        public List<Tag> Tags { get; set; } = new();
    }
}

