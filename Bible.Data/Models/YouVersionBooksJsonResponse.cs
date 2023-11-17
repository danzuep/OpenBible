using System.Text.Json.Serialization;

namespace Bible.Data.Models
{
    public class YouVersionBooksJsonResponse
    {
        [JsonPropertyName("books")]
        public YouVersionBookJsonResponse[] Books { get; set; } = [];
    }

    public class YouVersionBookJsonResponse
    {
        [JsonPropertyName("book")]
        public string Book { get; set; } = string.Empty;
        [JsonPropertyName("aliases")]
        public string[] Aliases { get; set; } = [];
        [JsonPropertyName("chapters")]
        public int Chapters { get; set; }
    }
}
