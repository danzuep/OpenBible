using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bible.Backend
{
    public class JsonParser : IDeserialize
    {
        private readonly ILogger<JsonParser> _logger;

        public JsonParser(ILogger<JsonParser>? logger = null)
        {
            _logger = logger ?? NullLogger<JsonParser>.Instance;
        }

        public T? Deserialize<T>(string filePath)
        {
            var jsonContent = File.ReadAllText(filePath);
            var deserialized = JsonSerializer.Deserialize<T>(jsonContent);
            return deserialized;
        }

        public void Serialize<T>(T data, string jsonOutputPath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(data, options);
            File.WriteAllText(jsonOutputPath, jsonString);
        }
    }
}