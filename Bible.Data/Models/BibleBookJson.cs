using System.Text.Json.Serialization;

namespace Bible.Data.Models
{
    public class SimpleBookJson
    {
        [JsonPropertyName("book")]
        public string? Book { get; set; }

        [JsonPropertyName("chapters")]
        public SimpleChapterJson[] Chapters { get; set; } = [];
    }

    public class SimpleChapterJson
    {
        [JsonPropertyName("chapter")]
        public string? Chapter { get; set; }

        [JsonPropertyName("verses")]
        public SimpleVerseJson[] Verses { get; set; } = [];
    }

    public class SimpleVerseJson
    {
        [JsonPropertyName("verse")]
        public string? Verse { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}