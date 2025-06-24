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
            var uglifiedDoc = GetUglifiedXml(xDocument);
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = uglifiedDoc.CreateReader())
            {
                var result = (T?)serializer.Deserialize(reader);
                return result;
            }
        }

        private async Task<XDocument> GetUglifiedXmlStreamAsync(Stream inputStream, CancellationToken cancellationToken = default)
        {
            var doc = await XDocument.LoadAsync(inputStream, LoadOptions.None, cancellationToken);

            var outputStream = new MemoryStream();

            var settings = new XmlWriterSettings
            {
                Indent = false,
                NewLineHandling = NewLineHandling.None,
                Encoding = System.Text.Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            using (var writer = XmlWriter.Create(outputStream, settings))
            {
                await doc.WriteToAsync(writer, cancellationToken);
            }

            outputStream.Position = 0; // Reset to beginning

            // Load the uglified XML back into a new XDocument (this normalizes whitespace as serialized)
            var uglifiedDoc = await XDocument.LoadAsync(outputStream, Settings, cancellationToken);

            return uglifiedDoc;
        }

        private XDocument GetUglifiedXml(XDocument xDocument)
        {
            var outputStream = new MemoryStream();

            var settings = new XmlWriterSettings
            {
                Indent = false,
                NewLineHandling = NewLineHandling.None,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = true
            };

            using (var writer = XmlWriter.Create(outputStream, settings))
            {
                xDocument.WriteTo(writer);
            }

            outputStream.Position = 0; // Reset to beginning

            var uglifiedDoc = XDocument.Load(outputStream, Settings);

            return uglifiedDoc;
        }
    }
}