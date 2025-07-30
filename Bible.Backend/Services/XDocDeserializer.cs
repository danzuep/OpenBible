using System.Xml.Linq;
using System.Xml.Serialization;
using Bible.Backend.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bible.Backend.Services
{
    public class XDocDeserializer : IDeserializer
    {
        private readonly ILogger<XDocDeserializer> _logger;

        public XDocDeserializer(ILogger<XDocDeserializer>? logger = null)
        {
            _logger = logger ?? NullLogger<XDocDeserializer>.Instance;
        }

        public LoadOptions Settings { get; set; } = LoadOptions.PreserveWhitespace;

        public T? DeserializeXml<T>(string xml)
        {
            using var reader = new StringReader(xml);
            var xdoc = XDocument.Load(reader, Settings);
            var deserialized = Deserialize<T>(xdoc);
            _logger.LogDebug($"Deserialized XML of length {xml.Length}");
            return deserialized;
        }

        public T? Deserialize<T>(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var xdoc = XDocument.Load(stream, Settings);
            var deserialized = Deserialize<T>(xdoc);
            _logger.LogDebug($"Deserialized {filePath}");
            return deserialized;
        }

        public async Task<TOut?> DeserializeAsync<TIn, TOut>(string filePath, Func<TIn?, TOut?> transform, CancellationToken cancellationToken = default)
        {
            var deserialized = await DeserializeAsync<TIn>(filePath, cancellationToken);
            return transform(deserialized);
        }

        public async Task<T?> DeserializeAsync<T>(string filePath, CancellationToken cancellationToken)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var xdoc = await XDocument.LoadAsync(stream, Settings, cancellationToken);
            var deserialized = Deserialize<T>(xdoc);
            _logger.LogDebug($"Deserialized {filePath}");
            return deserialized;
        }

        private static T? Deserialize<T>(XDocument xDocument)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = xDocument.CreateReader())
            {
                var result = (T?)serializer.Deserialize(reader);
                return result;
            }
        }
    }
}