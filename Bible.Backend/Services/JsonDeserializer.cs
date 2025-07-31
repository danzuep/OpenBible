using System.Reflection;
using System.Text.Json;
using Bible.Backend.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bible.Backend.Services
{
    public class JsonDeserializer : IDeserializer
    {
        private readonly ILogger<JsonDeserializer> _logger;

        public JsonDeserializer(ILogger<JsonDeserializer>? logger = null)
        {
            _logger = logger ?? NullLogger<JsonDeserializer>.Instance;
        }

        public T? Deserialize<T>(string filePath)
        {
            var jsonContent = File.ReadAllText(filePath);
            var deserialized = JsonSerializer.Deserialize<T>(jsonContent);
            _logger.LogDebug($"Deserialized {filePath}");
            return deserialized;
        }

        public async Task<TOut?> DeserializeAsync<TIn, TOut>(string filePath, Func<TIn?, TOut?> transform, CancellationToken cancellationToken = default)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var options = null as JsonSerializerOptions;
            var deserialized = await JsonSerializer.DeserializeAsync<TIn>(stream, options, cancellationToken);
            return transform(deserialized);
        }

        public async Task<TOut?> DeserializeResourceAsync<TIn, TOut>(string resourceName, Func<TIn?, TOut?> transform, CancellationToken cancellationToken = default)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var fileStream = assembly.GetManifestResourceStream(resourceName);
            if (fileStream == null) return default;
            var deserialized = await JsonSerializer.DeserializeAsync<TIn>(fileStream);
            return transform(deserialized);
        }

        public void Serialize<T>(T data, string jsonOutputPath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(data, options);
            File.WriteAllText(jsonOutputPath, jsonString);
        }
    }
}