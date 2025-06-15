using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Bible.Backend.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bible.Backend.Services
{
    public class XmlParser : IDeserialize
    {
        private readonly ILogger<XmlParser> _logger;

        public XmlParser(ILogger<XmlParser>? logger = null)
        {
            _logger = logger ?? NullLogger<XmlParser>.Instance;
        }

        public XmlReaderSettings Settings { get; set; } = new XmlReaderSettings
        {
            IgnoreWhitespace = false,
            DtdProcessing = DtdProcessing.Ignore
        };

        public T? Deserialize<T>(XmlReader xmlReader)
        {
            var serializer = new XmlSerializer(typeof(T)); // XmlRoot attribute must be set
            var result = (T?)serializer.Deserialize(xmlReader);
            return result;
        }

        public T? DeserializeXml<T>(string xml)
        {
            using (var stringReader = new StringReader(xml))
            using (var xmlReader = XmlReader.Create(stringReader, Settings))
            {
                return Deserialize<T>(xmlReader);
            }
        }

        public T? Deserialize<T>(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var xmlReader = XmlReader.Create(stream, Settings))
            {
                return Deserialize<T>(xmlReader);
            }
        }
    }
}