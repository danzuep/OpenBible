using System.Text.Json;
using System.Xml;
using Bible.Backend.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Formatting = Newtonsoft.Json.Formatting;

namespace Bible.Backend.Services
{
    public class XmlConverter
    {
        private readonly ILogger _logger;

        public XmlConverter(ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
        }

        public ILogger Logger => _logger;

        public async Task ParseUnihanAsync()
        {
            (var sitePath, var assetPath) = GetPaths();
            var inputPath = Path.Combine(sitePath, "Unihan_Readings.txt");
            var outputPath = Path.Combine(sitePath, "Unihan_Readings.json");
            var unihanSerializer = new UnihanSerializer();
            await unihanSerializer.ParseAsync(inputPath, outputPath);
        }

        public async Task<UnihanLookup> LoadUnihanAsync()
        {
            (var sitePath, var assetPath) = GetPaths();
            var inputPath = Path.Combine(sitePath, "Unihan_Readings.txt");
            var outputPath = Path.Combine(sitePath, "Unihan_Readings.json");
            if (File.Exists(outputPath))
            {
                try
                {
                    var unihanText = await File.ReadAllTextAsync(outputPath);
                    var deserialized = JsonSerializer.Deserialize<UnihanLookup>(unihanText);
                    if (deserialized != null)
                    {
                        return deserialized;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, ex.Message);
                }
            }
            var parser = new UnihanParserService();
            var unihan = await parser.ParseAsync<UnihanLookup>(inputPath, outputPath);
            return unihan;
        }

        public void Visitor<T>(Func<T, string, string> function, string suffix = "-usx", string? sample = null)
        {
            (var sitePath, var assetPath) = GetPaths();
            this._logger.LogInformation(assetPath);

            var deserializer = new XDocDeserializer();
            var usxParser = new UsxVersionParser(deserializer);

            foreach (var versionPath in Directory.EnumerateDirectories(sitePath))
            {
                var suffixLength = versionPath.EndsWith(suffix) ? suffix.Length : 0;
                var versionName = Path.GetFileName(versionPath)[..^suffixLength];
                this._logger.LogInformation(versionName);
                if (!string.IsNullOrEmpty(sample) && !versionName.StartsWith(sample))
                {
                    continue;
                }

                var outputPath = Path.Combine(assetPath, versionName);
                Directory.CreateDirectory(outputPath);

                var books = usxParser.Enumerate<T>(versionPath);

                foreach (var book in books)
                {
                    var text = function(book, outputPath);
                    if (text != null && !string.IsNullOrEmpty(sample))
                    {
                        this._logger.LogInformation($"{versionName}-{book}");
                        return;
                    }
                }
            }
        }

        public async Task XmlMetadataToJsonAsync(string suffix = "-usx", bool isTest = false)
        {
            (var sitePath, var assetPath) = GetPaths();
            _logger.LogInformation(assetPath);

            foreach (var versionPath in Directory.EnumerateDirectories(sitePath))
            {
                var suffixLength = versionPath.EndsWith(suffix) ? suffix.Length : 0;
                var versionName = Path.GetFileName(versionPath)[..^suffixLength];
                _logger.LogInformation(versionName);
                if (isTest && !versionName.StartsWith("eng-WEBBE"))
                {
                    continue;
                }

                var xmlFilePath = Directory.EnumerateFiles(versionPath, "metadata.xml").FirstOrDefault();
                if (xmlFilePath == null)
                {
                    return;
                }
                _logger.LogInformation(xmlFilePath);

                var textVersionPath = Path.Combine(assetPath, versionName);
                _logger.LogInformation(textVersionPath);

                var outputPath = Directory.Exists(textVersionPath) ?
                    Path.Combine(textVersionPath, "_metadata.json") :
                    $"{textVersionPath}_metadata.json";

                if (!File.Exists(outputPath))
                {
                    var json = ConvertXmlToJson(xmlFilePath, outputPath, omitRootObject: false);
                    await File.WriteAllTextAsync(outputPath, json);
                }
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

            var json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(xmlDoc, Formatting.Indented, omitRootObject);

            return json;
        }

        private static (string, string) GetPaths(string? biblePath = null)
        {
            biblePath ??= GetBiblePath();
            var sitePath = Path.Combine(biblePath, "_site");
            var assetPath = Path.Combine(biblePath, "texts");
            return (sitePath, assetPath);
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
