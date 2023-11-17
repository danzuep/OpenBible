using System.Text.Json.Serialization;

namespace Bible.Data.Models
{
    public class YouVersionVersesJsonResponse
    {
        [JsonPropertyName("data")]
        public Data? Data { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("reference")]
        public string? Reference { get; set; }

        [JsonPropertyName("content")]
        public Content[]? Content { get; set; }

        [JsonPropertyName("copyright")]
        public string? Copyright { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("items")]
        public Item[]? Items { get; set; }
    }

    public class Item
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
