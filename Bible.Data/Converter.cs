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
                Translation = version.Trim(),
                BookName = jsonBook.Name
            };
            var chapters = GetBibleChapters(jsonBook.Content, reference);
            var book = new BibleBook(reference, chapters)
            {
                Aliases = [jsonBook.Abbreviation]
            };
            return book;
        }

        public static BibleBook? GetBibleBook(this IEnumerable<BibleBook> bibleBooks, string bookName)
        {
            if (string.IsNullOrWhiteSpace(bookName)) return null;
            bookName = bookName.Trim();
            var book = bibleBooks.FirstOrDefault(book =>
                bookName.Equals(book.Reference.BookName, StringComparison.OrdinalIgnoreCase) ||
                (book.Aliases != null && book.Aliases.Contains(bookName, StringComparer.OrdinalIgnoreCase)));
            return book;
        }

        private static IReadOnlyList<BibleChapter> GetBibleChapters(string[][] content, BibleReference bibleReference)
        {
            var chapters = content.Select((chapterContent, chapterIndex) =>
            {
                var chapterNumber = chapterIndex + _indexOffset;
                var chapterReference = chapterNumber.ToString(CultureInfo.InvariantCulture);
                var reference = new BibleReference(bibleReference) { Reference = chapterReference };
                var chapter = new BibleChapter
                {
                    ChapterNumber = chapterNumber,
                    Reference = reference,
                    Verses = chapterContent.Select((verseText, verseIndex) =>
                    {
                        var verseNumber = verseIndex + _indexOffset;
                        var verse = new BibleVerse
                        {
                            Number = verseNumber,
                            Reference = reference,
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
            if (string.IsNullOrWhiteSpace(query)) return [];
            query = query.Trim();
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
