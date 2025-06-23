using System.Xml;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace Bible.Downloader
{
    public static class XmlJsonConverter
    {
        public static async Task<string> ConvertXmlToJsonAsync(string xmlPath, string outputPath, bool omitRootObject = false)
        {
            if (string.IsNullOrWhiteSpace(xmlPath))
                throw new ArgumentException("Input XML path cannot be null or empty.", nameof(xmlPath));

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be null or empty.", nameof(outputPath));

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            if (xmlDoc.FirstChild?.NodeType == XmlNodeType.XmlDeclaration)
            {
                xmlDoc.RemoveChild(xmlDoc.FirstChild);
            }

            var json = JsonConvert.SerializeXmlNode(xmlDoc, Formatting.Indented, omitRootObject);

            await File.WriteAllTextAsync(outputPath, json);

            return json;
        }
    }
}
