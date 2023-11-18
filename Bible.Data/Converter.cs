using Bible.Core.Models;
using Bible.Data.Models;
using System.Collections.Concurrent;
using System.Globalization;

namespace Bible.Data
{
    public static class Converter
    {
        const int _indexOffset = 1;

        public static BibleBook GetBibleBook(this SimpleBibleBookJson jsonBook, string version)
        {
            var reference = new BibleReference
            {
                Translation = version,
                BookName = jsonBook.Name,
                Aliases = [jsonBook.Abbreviation]
            };
            var chapters = GetBibleChapters(jsonBook.Content, reference);
            var book = new BibleBook(reference, chapters);
            return book;
        }

        private static IReadOnlyList<BibleChapter> GetBibleChapters(string[][] content, BibleReference bibleReference)
        {
            var chapters = content.Select((chapterContent, chapterIndex) =>
            {
                var chapterNumber = chapterIndex + _indexOffset;
                var chapterReference = chapterNumber.ToString(CultureInfo.InvariantCulture);
                var chapter = new BibleChapter
                {
                    ChapterNumber = chapterNumber,
                    Reference = new(bibleReference) { Reference = chapterReference },
                    Verses = chapterContent.Select((verseText, verseIndex) =>
                    {
                        var verseNumber = verseIndex + _indexOffset;
                        var verseReference = $"{chapterNumber}:{verseNumber}";
                        var verse = new BibleVerse
                        {
                            VerseNumber = verseNumber,
                            Reference = new(bibleReference) { Reference = verseReference },
                            Text = verseText
                        };
                        return verse;
                    }).ToArray()
                };
                return chapter;
            });
            return chapters.ToArray();
        }

        public static async Task<IReadOnlyCollection<BibleVerse>> SearchBibleAsync(this IEnumerable<BibleBook> books, string query)
        {
            var verses = new ConcurrentBag<BibleVerse>();
            await Task.WhenAll(books.Select(async (book) =>
            {
                await Task.WhenAll(book.Chapters.Select(async (chapter) =>
                {
                    await Task.WhenAll(chapter.Verses.Select(async (verse) =>
                    {
                        await Task.CompletedTask;
                        if (verse.Text.Contains(query, StringComparison.OrdinalIgnoreCase))
                        {
                            verses.Add(verse);
                        }
                    }));
                }));
            }));
            return verses;
        }
    }
}
