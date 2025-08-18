using System.Text;
using System.Text.Json;
using Bible.Backend.Abstractions;
using Bible.Core.Models.Meta;
using Bible.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Unihan.Models;
using Unihan.Services;

namespace Bible.Backend.Services
{
    public sealed class UnihanService
    {
        private readonly ILogger _logger;
        private readonly IStorageService _storageService;

        public UnihanService(IStorageService storageService, ILogger? logger = null)
        {
            _storageService = storageService;
            _logger = logger ?? NullLogger.Instance;
        }

        private static UnihanLookup? _unihan;
        public UnihanLookup? Unihan => _unihan;

        private static readonly string _filePath = "Unihan_Readings.json";

        public static async Task<UnihanLanguage> GetUnihanAsync(string isoLanguage, string? fileName = null)
        {
            UnihanLanguage unihan = new(isoLanguage);
            if (unihan.Field != UnihanField.Unknown)
            {
                unihan.Dictionary = await ResourceHelper.GetFromJsonAsync<UnihanLookup>(fileName ?? _filePath);
            }
            return unihan;
        }

        private async ValueTask GetStoredUnihanAsync()
        {
            if (_unihan != null) return;
            var key = nameof(UnihanLookup);
            _unihan = await _storageService.GetSerializedItemAsync<UnihanLookup>(key);
            if (_unihan == null)
            {
                _unihan = await ResourceHelper.GetFromJsonAsync<UnihanLookup>(_filePath);
                if (_unihan != null)
                {
                    _ = _storageService.SetSerializedItemAsync(key, _unihan);
                }
            }
        }

        public static async Task<WordsWithMetadata> ParseAsync(string text, UnihanField unihanField)
        {
            _unihan = await ResourceHelper.GetFromJsonAsync<UnihanLookup>(_filePath);
            return ParseWords(text, unihanField, _unihan);
        }

        public async Task<WordsWithMetadata> ParseUnihanRunesAsync(string text, string isoLanguage)
        {
            var language = new UnihanLanguage(isoLanguage);
            return await ParseUnihanRunesAsync(text, language.Field);
        }

        public async Task<WordsWithMetadata> ParseUnihanRunesAsync(string text, UnihanField unihanField)
        {
            await GetStoredUnihanAsync();
            return ParseWords(text, unihanField, _unihan);
        }

        public static string ParseToString(IEnumerable<WordMetadata> metadata)
        {
            var stringBuilder = new StringBuilder();
            foreach (var meta in metadata)
            {
                stringBuilder.AppendFormat("{0}: ", meta.Text.ToString());
                stringBuilder.AppendLine(string.Join("; ", meta.Metadata));
            }
            return stringBuilder.ToString();
        }

        public static WordsWithMetadata ParseWords(string text, UnihanField unihanField, UnihanLookup? unihanLookup)
        {
            return new WordsWithMetadata(ParseRunes(text, unihanField, unihanLookup));
        }

        public static IEnumerable<WordMetadata> ParseRunes(string text, UnihanField unihanField, UnihanLookup? unihanLookup)
        {
            if (string.IsNullOrWhiteSpace(text)) yield break;
            if (unihanLookup == null)
            {
                yield return new WordMetadata(text);
                yield break;
            }
            foreach (Rune rune in text.EnumerateRunes())
            {
                yield return ParseRune(rune, unihanField, unihanLookup);
            }
        }

        private static WordMetadata ParseRune(Rune rune, UnihanField unihanField, UnihanLookup unihanLookup)
        {
            if (unihanLookup.TryGetValue(rune.Value, out var metadata) &&
                metadata.TryGetValue(unihanField, out var values))
            {
                return new WordMetadata(rune.ToString(), values);
            }
            return new WordMetadata(rune.ToString());
        }

        public static async Task ParseUnihanAsync()
        {
            (var sitePath, var assetPath) = XmlConverter.GetPaths();
            var inputPath = Path.Combine(sitePath, "Unihan_Readings.txt");
            var outputPath = Path.Combine(sitePath, "Unihan_Readings.json");
            var unihanSerializer = new UnihanSerializer();
            await unihanSerializer.ParseAsync(inputPath, outputPath);
        }

        public static async Task<UnihanLookup> LoadUnihanAsync()
        {
            (var sitePath, var assetPath) = XmlConverter.GetPaths();
            var inputPath = Path.Combine(sitePath, "Unihan_Readings.txt");
            var outputPath = Path.Combine(sitePath, "Unihan_Readings.json");
            if (File.Exists(outputPath))
            {
                var unihanText = await File.ReadAllTextAsync(outputPath);
                var deserialized = JsonSerializer.Deserialize<UnihanLookup>(unihanText);
                if (deserialized != null)
                {
                    return deserialized;
                }
            }
            var parser = new UnihanParserService();
            var unihan = await parser.ParseAsync<UnihanLookup>(inputPath, outputPath);
            return unihan;
        }
    }
}
