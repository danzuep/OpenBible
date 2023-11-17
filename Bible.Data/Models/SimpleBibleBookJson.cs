using System.Text.Json.Serialization;

namespace Bible.Data.Models
{
    public class SimpleBibleBookJson
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("Abbreviation")]
        public string Abbreviation { get; set; } = default!;

        [JsonPropertyName("Chapters")]
        public string[][] Content { get; set; } = default!;
    }
}