using Bible.Core.Models;
using Bible.Data.Models;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace Bible.Data
{
    public static class Converter
    {
        const int _indexOffset = 1;

        public static BibleBook GetBibleBook(this SimpleBibleBookJson jsonBook, string version)
        {
            var reference = new BibleReference
            {
                Version = version.Trim(),
                BookName = jsonBook.Name
            };
            var chapters = GetBibleChapters(jsonBook.Content, reference);
            var book = new BibleBook
            {
                Reference = reference,
                Chapters = chapters,
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
                    Id = chapterNumber,
                    Reference = reference,
                    Verses = chapterContent.Select((verseText, verseIndex) =>
                    {
                        var verseNumber = verseIndex + _indexOffset;
                        var verse = new BibleVerse
                        {
                            Id = verseNumber,
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

        public static (string, BibleFootnote[]) SplitVerseFootnotes(this BibleVerse bibleVerse, char start = '{', char end = '}')
        {
            if (string.IsNullOrEmpty(bibleVerse?.Text))
                return (string.Empty, []);
            var verse = new StringBuilder("<sup>")
                .Append(bibleVerse.Id).Append("</sup>");
            try
            {
                int noteStartIndex = bibleVerse.Text.IndexOf(start);
                if (noteStartIndex == -1)
                    return ($"{verse}{bibleVerse.Text}", []);
                int fragmentStartIndex = 0;
                string fragment;
                List<BibleFootnote> footnotes = new();
                do
                {
                    fragment = bibleVerse.Text[fragmentStartIndex..noteStartIndex];
                    verse.Append(fragment);
                    var noteEndIndex = bibleVerse.Text.IndexOf(end, noteStartIndex);
                    if (noteEndIndex == -1)
                        break;
                    fragmentStartIndex = noteEndIndex + 1;
                    var footnote = bibleVerse.Text[++noteStartIndex..noteEndIndex];
                    var fnChar = (char)('a' + footnotes.Count);
                    var fnId = $"{bibleVerse.Id}{fnChar}";
                    var page = bibleVerse.Reference.ToPath();
                    verse.Append("<sup><a href=\"").Append(page)
                        .Append("#footnote-").Append(fnId)
                        .Append("\">[").Append(fnChar).Append("]</a></sup>");
                    footnotes.Add(new BibleFootnote(fnId, footnote));
                    // $"<li id=\"footnote-{fnId}\">[{fnId}] {footnote}</li>";
                    noteStartIndex = bibleVerse.Text.IndexOf(start, noteEndIndex);
                }
                while (noteStartIndex != -1);
                fragment = bibleVerse.Text[fragmentStartIndex..];
                verse.Append(fragment);
                return (verse.ToString(), footnotes.ToArray());
            }
            catch
            {
                System.Diagnostics.Debugger.Break();
                return ($"{verse}{bibleVerse.Text}", []);
            }
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
