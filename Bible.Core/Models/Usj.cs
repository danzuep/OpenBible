using System.Text.Json.Serialization;

namespace Bible.Core.Models
{
    public class Usj
    {
        [JsonPropertyName("usx")]
        public string UsxVersion { get; set; }

        [JsonPropertyName("code")]
        public string BookCode { get; set; }

        [JsonPropertyName("book")]
        public string BookVersion { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        [JsonPropertyName("contents")]
        public IList<ContentItem> Contents { get; set; }

        public Usj()
        {
            Metadata = new Dictionary<string, string>();
            Contents = new List<ContentItem>();
        }
    }

    public class ContentItem
    {
        public ContentItem(string type, string value)
        {
            Category = type;
            Value = value;
        }

        [JsonPropertyName("c")]
        public string Category { get; set; }

        [JsonPropertyName("v")]
        public string Value { get; set; }
    }
}
