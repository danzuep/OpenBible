using System.Globalization;
using Bible.Backend.Models;
using Bible.Backend.Visitors;
using Bible.Core.Models;
using Bible.Core.Models.Scripture;
using Bible.Data;

namespace Bible.Backend.Services
{
    public class StreamDataService
    {
        private readonly ScriptureBookMetadata _scriptureBookMetadata;

        public StreamDataService(ScriptureBookMetadata scriptureBookMetadata)
        {
            _scriptureBookMetadata = scriptureBookMetadata;
        }

        public static StreamDataService Create(string? isoLanguage = "eng", string? bibleVersion = "OCCB", string? bookCode = "3JN")
        {
            var metadata = new ScriptureBookMetadata
            {
                IsoLanguage = isoLanguage ?? throw new ArgumentNullException(nameof(isoLanguage)),
                Version = bibleVersion ?? throw new ArgumentNullException(nameof(bibleVersion)),
                Id = bookCode ?? throw new ArgumentNullException(nameof(bookCode))
            };
            return new StreamDataService(metadata);
        }

        public static IAsyncEnumerator<BibleChapter> GetAsyncEnumerator(string isoLanguage = "eng", string bibleVersion = "OCCB", string bookCode = "3JN")
        {
            var service = Create(isoLanguage, bibleVersion, bookCode);
            return service.StreamChaptersAsync().GetAsyncEnumerator();
        }

        public async IAsyncEnumerable<BibleChapter> StreamChaptersAsync()
        {
            var bibleBook = await LoadBookAsync(_scriptureBookMetadata.IsoLanguage, _scriptureBookMetadata.Version, _scriptureBookMetadata.Id);
            if (bibleBook == null)
            {
                yield break;
            }
            foreach (var bibleChapter in bibleBook.Chapters)
            {
                if (bibleChapter != null)
                {
                    yield return bibleChapter;
                }
            }
        }

        public async Task<BibleChapter?> FetchChapterAsync(int chapterNumber)
        {
            var bibleChapter = await LoadChapterAsync(_scriptureBookMetadata.IsoLanguage, _scriptureBookMetadata.Version, _scriptureBookMetadata.Id, chapterNumber);
            return bibleChapter;
        }

        private BibleBook? _bibleBook;

        public async Task<BibleBook?> LoadBookAsync(string? isoLanguage = "eng", string? bibleVersion = "OCCB", string? bookName = "3JN")
        {
            if (string.IsNullOrWhiteSpace(isoLanguage) || string.IsNullOrWhiteSpace(bibleVersion) || string.IsNullOrWhiteSpace(bookName))
            {
                return null;
            }
            if (_bibleBook == null)
            {
                _bibleBook = await ParseBibleBookAsync(isoLanguage, bibleVersion, bookName).ConfigureAwait(false);
                //var book = await ParseScriptureBookAsync(isoLanguage, bibleVersion, bookName).ConfigureAwait(false);
                //_bibleBook = GetBibleBook(book);
            }
            return _bibleBook;
        }

        public async Task<BibleChapter?> LoadChapterAsync(string isoLanguage = "eng", string bibleVersion = "OCCB", string bookName = "3JN", int chapterNumber = 1)
        {
            var bibleBook = await LoadBookAsync(bibleVersion, bookName).ConfigureAwait(false);
            var bibleChapter = bibleBook?.Chapters.Where(c => c.Id == chapterNumber).FirstOrDefault();
            return bibleChapter;
        }

        static BibleBook? GetBibleBook(ScriptureBook? book)
        {
            if (book == null)
            {
                return null;
            }
            var reference = new BibleReference
            {
                Version = book.Metadata.Version,
                BookCode = book.Metadata.Id,
                BookName = book.Metadata.Name
            };
            var chapters = GetBibleChapters(book, reference);
            var aliases = book.Metadata.Segments.Select(alias => alias.Text).ToArray();
            var bibleBook = new BibleBook
            {
                Reference = reference,
                Chapters = chapters,
                Aliases = aliases
            };
            return bibleBook;
        }

        static IReadOnlyList<BibleChapter> GetBibleChapters(ScriptureBook? book, BibleReference bibleReference)
        {
            var chapters = book?.GetAllChapterRanges().Select((chapterKvp) =>
            {
                var chapterNumber = chapterKvp.Key;
                var chapterReference = chapterNumber.ToString(CultureInfo.InvariantCulture);
                var reference = new BibleReference(bibleReference) { Reference = chapterReference };
                var chapter = new BibleChapter
                {
                    Id = chapterNumber,
                    Reference = reference,
                    Verses = book.GetChapterVerseRanges(chapterKvp.Key).Select((verseKvp) =>
                    {
                        var verseNumber = verseKvp.Key;
                        var text = book.GetVerse(chapterNumber, verseNumber).ToVerseHtml();
                        var verse = new BibleVerse
                        {
                            Id = verseNumber,
                            Reference = reference,
                            Text = text
                        };
                        return verse;
                    }).ToArray()
                };
                return chapter;
            });
            return chapters?.ToArray() ?? [];
        }

        static async Task<BibleBook?> ParseBibleBookAsync(string language, string version, string book)
        {
            var metadata = new BibleBookMetadata
            {
                IsoLanguage = language,
                BibleVersion = version,
                BookCode = book
            };
            //var unihan = await GetUnihanAsync(language).ConfigureAwait(false);
            await using var stream = ResourceHelper.GetUsxBookStream(language, version, book);
            var bibleBook = await UsxToBibleBookVisitor.DeserializeAsync(stream, metadata, null);
            return bibleBook;
        }

        static async Task<ScriptureBook?> ParseScriptureBookAsync(string language, string version, string book)
        {
            // language = "zho-Hant"; version = "OCCB"; book = "3JN";
            var unihan = await UnihanHelper.GetUnihanAsync(language);
            var options = new UsxVisitorOptions { EnableRunes = unihan?.Field };
            await using var stream = ResourceHelper.GetUsxBookStream(language, version, book);
            var scriptureBook = await UsxToScriptureBookVisitor.DeserializeAsync(stream, unihan, options);
            if (scriptureBook != null)
            {
                scriptureBook.Metadata.IsoLanguage = language;
            }
            return scriptureBook;
        }
    }
}
