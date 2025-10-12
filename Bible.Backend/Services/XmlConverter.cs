using System.Xml;
using Bible.Backend.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Formatting = Newtonsoft.Json.Formatting;

namespace Bible.Backend.Services
{
    public class XmlConverter
    {
        private readonly IDeserializer _deserializer;
        private readonly ILogger _logger;

        public XmlConverter(IDeserializer? deserializer = null, ILogger? logger = null)
        {
            _deserializer = deserializer ?? new XDocDeserializer();
            _logger = logger ?? NullLogger.Instance;
        }

        public ILogger Logger => _logger;

        public void Visitor<T>(Func<T, string, string> function, string suffix = "-usx", string? sample = null)
        {
            (var sitePath, var assetPath) = GetPaths();
            this._logger.LogInformation(assetPath);

            var usxParser = new UsxVersionParser(_deserializer);

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
                this._logger.LogDebug(outputPath);

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

                var xmlFilePath = Directory.EnumerateFiles(versionPath, "metadata.xml").FirstOrDefault() ??
                    Directory.EnumerateFiles(versionPath, "extra/metadata.xml").FirstOrDefault();
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

        internal static (string, string) GetPaths(string? biblePath = null, string indir = "Bible.Data/usx", string outdir = "Bible.Data/Uploads")
        {
            biblePath ??= GetThisProjectPath();
            var sitePath = Path.Combine(biblePath, indir);
            var assetPath = Path.Combine(biblePath, outdir);
            return (sitePath, assetPath);
        }

        private static string GetThisProjectPath(string thisProject = "OpenBible")
        {
            var filePath = thisProject;
            do
            {
                filePath = Path.Combine("..", filePath);
            }
            while (!Directory.Exists(filePath));
            filePath = filePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            filePath = filePath[..^(thisProject.Length)];
            filePath = filePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            filePath = filePath[..^("..".Length)];
            System.Diagnostics.Debug.Assert(Directory.Exists(filePath), $"Project path not found: {filePath}");

            return filePath;
        }

        private static string GetBiblePath(string thisProject = "OpenBible", string bibleProjectName = "Bible")
        {
            thisProject = GetThisProjectPath(thisProject);
            var biblePath = Path.Combine(thisProject, "..", bibleProjectName);

            return biblePath;
        }
    }
}
