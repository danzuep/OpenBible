using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Bible.Backend.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bible.Backend.Services
{
    public class XDocParser : IDeserialize
    {
        private readonly ILogger<XDocParser> _logger;

        public XDocParser(ILogger<XDocParser>? logger = null)
        {
            _logger = logger ?? NullLogger<XDocParser>.Instance;
        }

        public LoadOptions Settings { get; set; } = LoadOptions.PreserveWhitespace;

        public T? DeserializeXml<T>(string xml)
        {
            using var reader = new StringReader(xml);
            var xdoc = XDocument.Load(reader, Settings);
            return Deserialize<T>(xdoc);
        }

        public T? Deserialize<T>(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var xdoc = XDocument.Load(stream, Settings);
            return Deserialize<T>(xdoc);
        }

        public async Task<T?> DeserializeAsync<T>(string filePath, CancellationToken cancellationToken)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var xdoc = await XDocument.LoadAsync(stream, Settings, cancellationToken);
            return Deserialize<T>(xdoc);
        }

        private T? Deserialize<T>(XDocument xDocument)
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