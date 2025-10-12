using System.Text;
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

        private static readonly string _filePath = "Unihan_Readings.json";

        private static string GetName(UnihanField field) => $"unihan_{field}";

        public static async Task<UnihanLanguage> GetUnihanAsync(string isoLanguage, bool dictionary = false, string? fileName = null)
        {
            UnihanLanguage unihan = new(isoLanguage);
            if (unihan.Field != UnihanField.Unknown)
            {
                if (dictionary)
                {
                    unihan.Dictionary = await ResourceHelper.GetFromJsonAsync<UnihanDictionary>($"{GetName(unihan.Field)}.json");
                }
                else
                {
                    unihan.Lookup = await ResourceHelper.GetFromJsonAsync<UnihanLookup>(fileName ?? _filePath);
                }
            }
            return unihan;
        }

        public static async Task<UnihanFieldDictionary?> GetUnihanFieldDictionaryAsync(string fileName = "unihan.json")
        {
            var unihan = await ResourceHelper.GetFromJsonAsync<UnihanFieldDictionary>(fileName);
            return unihan;
        }

        public async Task<UnihanDictionary?> GetUnihanDictionaryAsync(UnihanField unihanField)
        {
            var key = GetName(unihanField);
            var unihan = await _storageService.GetSerializedItemAsync<UnihanDictionary>(key);
            if (unihan == null)
            {
                unihan = await ResourceHelper.GetFromJsonAsync<UnihanDictionary>($"{key}.json");
                _ = _storageService.SetSerializedItemAsync(key, unihan);
            }
            unihan?.Field = unihanField;
            return unihan;
        }

        public async Task<IList<string>> ParseAsync(int codepoint, UnihanField unihanField)
        {
            var unihan = await GetUnihanDictionaryAsync(unihanField);
            if (unihan == null || !unihan.TryGetValue(codepoint, out var values)) return Array.Empty<string>();
            return values;
        }

        public Func<int, IList<string>> GetEnrichDelegate(UnihanDictionary unihanDictionary)
        {
            return codepoint =>
            {
                if (unihanDictionary == null) return Array.Empty<string>();
                var metadata = unihanDictionary.GetValue(codepoint);
                return metadata;
            };
        }

        public static async Task<WordsWithMetadata> ParseAsync(string text, string isoLanguage)
        {
            var language = new UnihanLanguage(isoLanguage);
            return await ParseAsync(text, language.Field);
        }

        public static async Task<WordsWithMetadata> ParseAsync(string text, UnihanField unihanField)
        {
            var fileName = $"{GetName(unihanField)}.json";
            var unihan = await ResourceHelper.GetFromJsonAsync<UnihanDictionary>(fileName);
            unihan?.Field = unihanField;
            return ParseWords(text, unihan);
        }

        public async Task<WordsWithMetadata> ParseUnihanRunesAsync(string text, string isoLanguage)
        {
            var language = new UnihanLanguage(isoLanguage);
            return await ParseUnihanRunesAsync(text, language.Field);
        }

        public async Task<WordsWithMetadata> ParseUnihanRunesAsync(string text, UnihanField unihanField)
        {
            var unihan = await GetUnihanDictionaryAsync(unihanField);
            return ParseWords(text, unihan);
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

        public static WordsWithMetadata ParseWords(string text, UnihanField unihanField, UnihanFieldDictionary? unihanLookup)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new WordsWithMetadata(Array.Empty<WordMetadata>());
            }
            if (unihanLookup == null || !unihanLookup.TryGetValue(unihanField, out var fieldMap))
            {
                return new WordsWithMetadata([new WordMetadata(text)]);
            }
            return ParseWords(text, fieldMap);
        }

        public static WordsWithMetadata ParseWords(string text, UnihanDictionary? unihan)
        {
            return new WordsWithMetadata(ParseRunes(text, unihan));
        }

        public static IEnumerable<WordMetadata> ParseRunes(string text, UnihanDictionary? unihan)
        {
            if (string.IsNullOrWhiteSpace(text)) yield break;
            if (unihan == null)
            {
                yield return new WordMetadata(text);
                yield break;
            }
            foreach (Rune rune in text.EnumerateRunes())
            {
                yield return ParseRune(rune, unihan);
            }
        }

        private static WordMetadata ParseRune(Rune rune, UnihanDictionary unihan)
        {
            if (unihan.TryGetValue(rune.Value, out var values))
            {
                return new WordMetadata(rune.ToString(), values);
            }
            return new WordMetadata(rune.ToString());
        }
    }
}
