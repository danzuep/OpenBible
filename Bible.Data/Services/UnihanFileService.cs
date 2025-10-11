using System.IO;
using System.IO.Compression;
using System.Text.Json;
using Bible.Backend.Services;
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

        public UnihanFileService(IOptions<UnihanSplitterOptions> optionsAccessor)
        {
            _options = optionsAccessor.Value;
        }

        public static async Task<string> ParseUnihanReadingsToFileAsync(string fileName = "Unihan_Readings", IUnihanParserService? unihanParserService = null)
        {
            unihanParserService ??= new UnihanParserService();
            await using var inputStream = ResourceHelper.GetStreamFromExtension($"{fileName}.txt");
            await using var outputStream = await unihanParserService.ProcessStreamAsync(inputStream);
            var filePath = await ResourceHelper.WriteStreamAsync($"{fileName}.json", outputStream);
            return filePath;
        }

        public async Task<IList<string>> SplitUnihanReadingsToFilesAsync()
        {
            await using var inputStream = ResourceHelper.GetStreamFromExtension(_options.InputPath);
            var unihan = await UnihanParserService.ParseAsync<UnihanFieldDictionary>(inputStream);

            var results = new List<string>();
            
            foreach (var kvp in unihan)
            {
                if (kvp.Value == null) continue;
                var filePaths = await SplitAsync(kvp.Key, kvp.Value);
                results.AddRange(filePaths);
            }
            return results;

            async Task<IList<string>> SplitAsync(UnihanField field, UnihanDictionary fieldMap)
            {
                var pages = new PaginatedUnihanDictionary(_options.PageSize, fieldMap);
                return await PaginateAsync(field, pages);
            }

            async Task<IList<string>> PaginateAsync(UnihanField field, PaginatedUnihanDictionary pages)
            {
                var results = new List<string>();

                foreach (var page in pages)
                {
                    int pageId = page.Key;
                    var pageItems = page.Value;
                    var fileName = $"{_options.Prefix}_{field}_{pageId:D4}.json";
                    var outputStream = await SerializeAsync(pageItems);
                    var filePath = await ResourceHelper.WriteStreamAsync(fileName, outputStream, normalizeFileName: false);
                    results.Add(filePath);
                }

                return results;
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
