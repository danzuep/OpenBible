using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bible.Data.Usx;

public class XmlParser
{
    private readonly ILogger<XmlParser> _logger;

    public XmlParser(ILogger<XmlParser>? logger = null)
    {
        _logger = logger ?? NullLogger<XmlParser>.Instance;
    }

    public T? Deserialize<T>(Stream stream, XmlReaderSettings? settings)
    {
        settings ??= new XmlReaderSettings
        {
            IgnoreWhitespace = false,
            DtdProcessing = DtdProcessing.Ignore
        };
        var serializer = new XmlSerializer(typeof(T));
        var result = (T?)serializer.Deserialize(stream);
        return result;
    }

    public T? DeserializeXml<T>(string xml, XmlReaderSettings? settings = null)
    {
        settings ??= new XmlReaderSettings
        {
            IgnoreWhitespace = false,
            DtdProcessing = DtdProcessing.Ignore
        };

        var serializer = new XmlSerializer(typeof(T)); // XML attributes must be set

        using (var stringReader = new StringReader(xml))
        using (var xmlReader = XmlReader.Create(stringReader, settings))
        {
            var result = (T?)serializer.Deserialize(xmlReader);
            return result;
        }
    }

    public T? Deserialize<T>(string filePath)
    {
        var serializer = new XmlSerializer(typeof(T)); // XmlRoot attribute must be set
        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            var result = Deserialize<T>(fs, null);
            return result;
        }
    }
}