using System.IO;
using System.IO.Compression;
using System.Text.Json;
using Bible.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Unihan.Models;
using Unihan.Services;

namespace Bible.Data.Services
{
    //public sealed class UnihanDictionary : Dictionary<int, IList<string>> { }
    //public sealed class UnihanFieldDictionary : Dictionary<UnihanField, UnihanDictionary> { }

    /// <summary>
    /// Splits UnihanLookup into per-field per-page JSON files.
    /// Optionally precompresses files to .gz and .br sidecars.
    /// </summary>
    public sealed class UnihanFileService
    {
        private const string _defaultFileName = "Unihan_Readings";
        private readonly UnihanSplitterOptions _options;
        private readonly ILogger _logger;

        public UnihanFileService(IOptions<UnihanSplitterOptions> optionsAccessor, ILogger<UnihanFileService>? logger = null)
        {
            _options = optionsAccessor.Value;
            _logger = logger ?? NullLogger<UnihanFileService>.Instance;
        }

        public UnihanFileService(ILogger logger, UnihanSplitterOptions? optionsAccessor = null)
        {
            _options = optionsAccessor ?? new();
            _logger = logger ?? NullLogger.Instance;
        }

        public static async Task<string> ParseUnihanReadingsToFileAsync(string fileName = _defaultFileName, IUnihanParserService? unihanParserService = null)
        {
            unihanParserService ??= new UnihanParserService();
            await using var inputStream = ResourceHelper.GetStreamFromExtension($"{fileName}.txt");
            await using var outputStream = await unihanParserService.ProcessStreamAsync(inputStream);
            var filePath = await ResourceHelper.WriteStreamAsync(outputStream, $"{fileName}.json");
            return filePath;
        }

        public static async Task<string> ParseUnihanReadingsToDictionaryAsync(string fileName = "unihan.json", IEnumerable<UnihanField>? fields = null)
        {
            if (fields is null || !fields.Any())
            {
                fields = [
                    UnihanField.kMandarin,
                    UnihanField.kCantonese,
                    UnihanField.kJapanese
                ];
            }
            await using var inputStream = ResourceHelper.GetStreamFromExtension($"{_defaultFileName}.txt");
            var unihan = await UnihanParserService.ParseAsync<UnihanFieldDictionary>(inputStream, fields);
            var filePath = await ResourceHelper.WriteJsonAsync(unihan, fileName);
            return filePath;
        }

        public async Task SplitUnihanReadingsToFilesAsync()
        {
            await using var inputStream = ResourceHelper.GetStreamFromExtension(_options.InputPath);
            var unihan = await UnihanParserService.ParseAsync<UnihanFieldDictionary>(inputStream);
            var progress = new Progress<string>(fileName => _logger.LogDebug($"File created: {fileName}"));

            foreach (var kvp in unihan)
            {
                if (kvp.Value == null) continue;
                var pages = new PaginatedUnihanDictionary(_options.PageSize, kvp.Value);
                await PaginateAsync(kvp.Key, pages, _options.Prefix, progress);
            }

            static async Task PaginateAsync(UnihanField field, PaginatedUnihanDictionary pages, string prefix, IProgress<string>? progress)
            {
                foreach (var page in pages)
                {
                    var fileName = $"{prefix}_{field}_{page.Key:D4}.json";
                    var filePath = await ResourceHelper.WriteJsonAsync(page.Value, fileName, normalizeFileName: false);
                    progress?.Report(filePath);
                }
            }
        }

        /// <summary>
        /// Create .gz and .br compressed copies of the input file.
        /// Overwrites existing .gz/.br files.
        /// </summary>
        public static async Task PrecompressFileAsync(string filePath)
        {
            // Gzip (.gz)
            var gzPath = filePath + ".gz";
            using (var originalStream = File.OpenRead(filePath))
            using (var compressedStream = File.Create(gzPath))
            using (var gzipStream = new GZipStream(compressedStream, CompressionLevel.Fastest))
            {
                await originalStream.CopyToAsync(gzipStream);
            }

            // Brotli (.br)
            var brPath = filePath + ".br";
            using (var originalStream = File.OpenRead(filePath))
            using (var compressedStream = File.Create(brPath))
            using (var brotliStream = new BrotliStream(compressedStream, CompressionLevel.Optimal))
            {
                await originalStream.CopyToAsync(brotliStream);
            }
        }
    }
}
