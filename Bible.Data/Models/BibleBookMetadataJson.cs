using System.Text.Json.Serialization;

namespace Bible.Data.Models
{
    public class BibleBookMetadataJson
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("chapters")]
        public int Chapters { get; set; }

        [JsonPropertyName("aliases")]
        public string[] Aliases { get; set; } = [];
    }
}