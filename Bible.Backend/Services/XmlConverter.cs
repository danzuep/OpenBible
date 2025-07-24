using System.Xml;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace Bible.Backend.Services
{
    public static class XmlConverter
    {
        public static void Visitor<T>(Func<T, string, string> function, ILogger logger, string suffix = "-usx", bool isTest = false)
        {
            var biblePath = GetBiblePath();
            var sitePath = Path.Combine(biblePath, "_site");
            var textsPath = Path.Combine(biblePath, "texts");
            logger.LogInformation(textsPath);

            var deserializer = new XDocParser();
            var usxParser = new UsxParser(deserializer);

            foreach (var versionPath in Directory.EnumerateDirectories(sitePath))
            {
                var suffixLength = versionPath.EndsWith(suffix) ? suffix.Length : 0;
                var versionName = Path.GetFileName(versionPath)[..^suffixLength];
                logger.LogInformation(versionName);
                if (isTest && !versionName.StartsWith("eng-WEBBE"))
                {
                    continue;
                }

                var outputPath = Path.Combine(textsPath, versionName);
                Directory.CreateDirectory(outputPath);

                var books = usxParser.Deserialize<T>(versionPath);

                foreach (var book in books)
                {
                    var text = function(book, outputPath);
                    if (isTest)
                    {
                        logger.LogInformation($"{versionName}-{book}");
                        return;
                    }
                }
            }
        }

        public static async Task XmlMetadataToJsonAsync(ILogger logger, string suffix = "-usx", bool isTest = false)
        {
            var biblePath = GetBiblePath();
            var sitePath = Path.Combine(biblePath, "_site");
            var textsPath = Path.Combine(biblePath, "texts");
            logger.LogInformation(textsPath);

            foreach (var versionPath in Directory.EnumerateDirectories(sitePath))
            {
                var suffixLength = versionPath.EndsWith(suffix) ? suffix.Length : 0;
                var versionName = Path.GetFileName(versionPath)[..^suffixLength];
                logger.LogInformation(versionName);
                if (isTest && !versionName.StartsWith("eng-WEBBE"))
                {
                    continue;
                }

                var xmlFilePath = Directory.EnumerateFiles(versionPath, "metadata.xml").FirstOrDefault();
                if (xmlFilePath == null)
                {
                    return;
                }
                logger.LogInformation(xmlFilePath);

                var textVersionPath = Path.Combine(textsPath, versionName);
                logger.LogInformation(textVersionPath);

                var outputPath = Directory.Exists(textVersionPath) ?
                    Path.Combine(textVersionPath, "_metadata.json") :
                    $"{textVersionPath}_metadata.json";

                var json = ConvertXmlToJson(xmlFilePath, outputPath, omitRootObject: false);

                await File.WriteAllTextAsync(outputPath, json);
            }
        }

        private static string ConvertXmlToJson(string xmlPath, string outputPath, bool omitRootObject = false)
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

            return json;
        }

        private static string GetBiblePath(string thisProject = "OpenBible", string bibleProjectName = "Bible")
        {
            do
            {
                thisProject = Path.Combine("..", thisProject);
            }
            while (!Directory.Exists(thisProject));

            var biblePath = Path.Combine(thisProject, "..", bibleProjectName);

            return biblePath;
        }
    }
}
