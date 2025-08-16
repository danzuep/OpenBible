using Bible.Backend.Adapters;
using Bible.Backend.Models;
using Bible.Backend.Visitors;
using Bible.Core.Models;
using Bible.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bible.Backend.Services
{
    public class BibleBookService
    {
        private static UnihanLookup? _unihan;
        private readonly Dictionary<BibleBookMetadata, BibleBook> _bibleBooks = new();

        private readonly ILogger _logger;

        public BibleBookService(ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
        }

        public UnihanLookup? Unihan => _unihan;

        public string? GetHtml(BibleBookMetadata bibleBookMetadata)
        {
            if (_bibleBooks.TryGetValue(bibleBookMetadata, out var bibleBook))
            {
                return bibleBook.GetHtml(_unihan);
            }
            return null;
        }

        public string? GetMarkdown(BibleBookMetadata bibleBookMetadata)
        {
            if (_bibleBooks.TryGetValue(bibleBookMetadata, out var bibleBook))
            {
                return bibleBook.GetMarkdown();
            }
            return null;
        }

        public async Task<BibleBook?> GetBibleBookAsync(string? language, string? version, string? book)
        {
            if (string.IsNullOrEmpty(language) || string.IsNullOrEmpty(version) || string.IsNullOrEmpty(book))
            {
                _logger.LogWarning("Invalid parameters for GetBibleBookAsync: language={Language}, version={Version}, book={Book}", language, version, book);
                return null;
            }
            var metadata = new BibleBookMetadata
            {
                IsoLanguage = language,
                BibleVersion = version,
                BookCode = book
            };
            return await GetBibleBookAsync(metadata);
        }

        public async Task<BibleBook?> GetBibleBookAsync(BibleBookMetadata bibleBookMetadata)
        {
            await using var stream = ResourceHelper.GetUsxBookStream(bibleBookMetadata);
            var bibleBook = await UsxToBibleBookVisitor.DeserializeAsync(stream, bibleBookMetadata);
            if (bibleBook != null)
            {
                _bibleBooks.TryAdd(bibleBookMetadata, bibleBook);
                if (_unihan == null)
                {
                    _unihan = await GetUnihanAsync(bibleBookMetadata.IsoLanguage);
                }
            }
            return bibleBook;
        }

        public static async Task<UnihanLookup?> GetUnihanAsync(string isoLanguage, string fileName = "Unihan_Readings.json")
        {
            UnihanLookup? unihan = null;
            if (UnihanLookup.NameUnihanLookup.TryGetValue(isoLanguage, out var unihanFields))
            {
                unihan = await ResourceHelper.GetFromJsonAsync<UnihanLookup>(fileName);
                if (unihan != null)
                {
                    unihan.IsoLanguage = isoLanguage;
                    unihan.Field = unihanFields.FirstOrDefault();
                }
            }
            return unihan;
        }
    }
}
