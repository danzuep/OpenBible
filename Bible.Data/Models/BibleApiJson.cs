using System.Text.Json.Serialization;

namespace Bible.Data.Models
{
    public class BibleApiJson
    {
        [JsonPropertyName("reference")]
        public string? Reference { get; set; }

        [JsonPropertyName("verses")]
        public BibleApiVerseJson[] Verses { get; set; } = [];

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("translation_id")]
        public string? TranslationId { get; set; }

        [JsonPropertyName("translation_name")]
        public string? TranslationName { get; set; }

        [JsonPropertyName("translation_note")]
        public string? TranslationNote { get; set; }
    }

    public class BibleApiVerseJson
    {
        [JsonPropertyName("book_id")]
        public string? BookId { get; set; }

        [JsonPropertyName("book_name")]
        public string? BookName { get; set; }

        [JsonPropertyName("chapter")]
        public int Chapter { get; set; }

        [JsonPropertyName("verse")]
        public int Verse { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}