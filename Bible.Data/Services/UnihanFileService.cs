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

        public static async Task<string> ParseUnihanReadingsToFileAsync(string fileName = "Unihan_Readings", IUnihanParserService? unihanParserService = null)
        {
            unihanParserService ??= new UnihanParserService();
            await using var inputStream = ResourceHelper.GetStreamFromExtension($"{fileName}.txt");
            await using var outputStream = await unihanParserService.ProcessStreamAsync(inputStream);
            var filePath = await ResourceHelper.WriteStreamAsync($"{fileName}.json", outputStream);
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
                    int pageId = page.Key;
                    var pageItems = page.Value;
                    var fileName = $"{prefix}_{field}_{pageId:D4}.json";
                    var outputStream = await SerializeAsync(pageItems);
                    var filePath = await ResourceHelper.WriteStreamAsync(fileName, outputStream, normalizeFileName: false);
                    progress?.Report(filePath);
                }
            }
        }

        private static async Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
        {
            var options = null as JsonSerializerOptions;
            var deserialized = await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken);
            return deserialized;
        }

        private static async Task<MemoryStream> SerializeAsync<T>(T target, CancellationToken cancellationToken = default)
        {
            var options = null as JsonSerializerOptions;
            var outputStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(outputStream, target, options, cancellationToken);
            outputStream.Position = 0;
            return outputStream;
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
