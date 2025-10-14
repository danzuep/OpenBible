using System.Text.Json;
using System.Text.Json.Serialization;
using Bible.Usx.Models;
using Bible.Usx.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Unihan.Models;
using Unihan.Services;

namespace Bible.Backend.Services
{
    public class XmlReaderDeserializer
    {
        private readonly UsxToUsjConverter _usxToUsjConverter;
        private readonly UsxParserFactory _parserFactory;
        private readonly ILogger _logger;

        public XmlReaderDeserializer(ILogger? logger = null, UsxToUsjConverter? usxToUsjConverter = null, UsxParserFactory? parserFactory = null)
        {
            _parserFactory = parserFactory ?? new UsxParserFactory();
            _usxToUsjConverter = usxToUsjConverter ?? new UsxToUsjConverter(_parserFactory);
            _logger = logger ?? NullLogger.Instance;
        }

        public async Task ParseVisitor(string filter = "zho", string fileType = "usx")
        {
            (var sitePath, var assetPath) = XmlConverter.GetPaths(outdir: "Bible.Data/usj");
            _logger.LogInformation(assetPath);

            var unihan = await UnihanService.GetUnihanFieldDictionaryAsync();
            if (unihan == null)
            {
                _logger.LogWarning("Unihan file not found.");
                return;
            }

            foreach (var versionPath in Directory.EnumerateDirectories(sitePath))
            {
                var suffix = $"-{fileType}";
                var suffixLength = versionPath.EndsWith(suffix) ? suffix.Length : 0;
                var versionName = Path.GetFileName(versionPath)[..^suffixLength];
                if (!versionName.StartsWith(filter))
                {
                    _logger.LogInformation("Skipped {VersionName}.", versionName);
                    continue;
                }
                _logger.LogInformation("Parsing {VersionName}.", versionName);

                var outputPath = Path.Combine(assetPath, versionName);
                Directory.CreateDirectory(outputPath);
                _logger.LogDebug(outputPath);

                var files = ParsingStrategy.GetFiles(versionPath, fileType);

                foreach (var file in files)
                {
                    await ParseToFileAsync(file, outputPath);
                }
            }

            async Task ParseToFileAsync(string filePath, string outputPath)
            {
                var fileName = Path.GetFileNameWithoutExtension(outputPath);
                var langScript = string.Join("-", fileName.Split('-').SkipLast(1));
                var type = UnihanLanguage.GetUnihanField(langScript);
                var unihanDictionary = unihan.GetValueOrDefault(type);
                if (unihanDictionary != null)
                {
                    _parserFactory.SetTextParser(unihanDictionary.GetValue);
                }
                var book = await DeserializeToUsjBookAsync(filePath);
                var outFilePath = Path.Combine(outputPath, $"{book?.Metadata.BookCode}.usj");
                await WriteJsonAsync(book, outFilePath);
                _logger.LogInformation("Book parsed to {Path}.", outFilePath);
            }
        }

        private static async Task WriteJsonAsync<T>(T input, string filePath, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            options ??= new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            await using var outputStream = new FileStream(filePath, FileMode.Create);
            await JsonSerializer.SerializeAsync(outputStream, input, options, cancellationToken);
        }

        public async Task<UsjBook?> DeserializeToUsjBookAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return await _usxToUsjConverter.ConvertUsxStreamToUsjBookAsync(stream, cancellationToken);
        }
    }
}