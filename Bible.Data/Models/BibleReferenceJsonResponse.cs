using System.Text.Json.Serialization;

namespace Bible.Data.Models
{
    public abstract class BibleResponse<T>
    {
        [JsonPropertyName("errors")]
        public object[]? Errors { get; set; }
        [JsonPropertyName("error_level")]
        public int ErrorLevel { get; set; }
        [JsonPropertyName("results")]
        public T[] Results { get; set; } = [];
    }

    public sealed class BibleReferenceJsonResponse : BibleResponse<BibleReferenceResults>
    {
        [JsonPropertyName("hash")]
        public string? Hash { get; set; }
        [JsonPropertyName("strongs")]
        public object[]? Strongs { get; set; }
        [JsonPropertyName("disambiguation")]
        public object[]? Disambiguation { get; set; }
        //[JsonPropertyName("paging")]
        //public BiblePagingResponse? Paging { get; set; }
        [JsonPropertyName("verse_index")]
        public Dictionary<string, int[]>? ChapterVerses { get; set; }
    }

    public class BiblePagingResponse
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("per_page")]
        public int PerPage { get; set; }

        [JsonPropertyName("current_page")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("last_page")]
        public int LastPage { get; set; }

        [JsonPropertyName("from")]
        public int From { get; set; }

        [JsonPropertyName("to")]
        public int To { get; set; }
    }

    public class BibleReferenceResults
    {
        [JsonPropertyName("book_id")]
        public int BookId { get; set; }
        [JsonPropertyName("book_name")]
        public string BookName { get; set; } = string.Empty;
        [JsonPropertyName("book_short")]
        public string? BookShort { get; set; }
        [JsonPropertyName("book_raw")]
        public string? BookRaw { get; set; }
        [JsonPropertyName("chapter_verse")]
        public string? ChapterVerse { get; set; }
        [JsonPropertyName("chapter_verse_raw")]
        public string? ChapterVerseRaw { get; set; }
        [JsonPropertyName("verses")]
        public Dictionary<string, Dictionary<string, Dictionary<string, BibleReference>>> Verses { get; set; } = new();
        [JsonPropertyName("verses_count")]
        public int VersesCount { get; set; }
        [JsonPropertyName("single_verse")]
        public bool SingleVerse { get; set; }
        [JsonPropertyName("nav")]
        public object[]? Nav { get; set; }
    }

    public class BibleReference
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("book")]
        public int Book { get; set; }
        [JsonPropertyName("chapter")]
        public int Chapter { get; set; }
        [JsonPropertyName("verse")]
        public int Verse { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
        [JsonPropertyName("italics")]
        public string? Italics { get; set; }
        [JsonPropertyName("claimed")]
        public bool Claimed { get; set; }
    }
}
