using System.Globalization;
using System.Text;
using Bible.Backend.Models;
using Bible.Core.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Bible.Backend.Adapters
{
    public static class UsxScriptureBookToBibleBookAdapter
    {
        public static BibleBook? ToBibleFormat(this UsxBook? book)
        {
            if (book == null)
            {
                return null;
            }

            var bibleReference = new BibleReference
            {
                BookName = book.Metadata.BookCode
            };

            var bibleBook = new BibleBook { Reference = bibleReference };
            var chapters = new List<BibleChapter>();
            var bibleChapter = new BibleChapter();
            var verses = new List<BibleVerse>();
            var stringBuilder = new StringBuilder();

            foreach (var content in book.Content)
            {
                if (content is UsxPara heading && heading.Style == "h" &&
                    heading.Content?.FirstOrDefault() is UsxHeading bookName)
                {
                    bibleReference.BookName = bookName.Text;
                    //bibleBook.Reference = bibleReference;
                }
                else if (content is UsxMarker chapterMarker)
                {
                    if (chapterMarker.Style == "c" && !string.IsNullOrEmpty(chapterMarker.Number))
                    {
                        bibleChapter = chapterMarker.SetBibleChapterReference(bibleReference);
                    }
                    else
                    {
                        bibleChapter.Verses = [.. verses];
                        chapters.Add(bibleChapter);
                        verses.Clear();
                    }
                }
                else if (content is UsxPara paragraph && paragraph.Style == "p")
                {
                    foreach (var verse in paragraph.ToBibleFormat(bibleReference, stringBuilder))
                    {
                        verses.Add(verse);
                    }
                }
            }

            bibleBook.Chapters = [.. chapters];
            chapters.Clear();

            return bibleBook;
        }

        private static BibleChapter SetBibleChapterReference(this UsxMarker chapterMarker, BibleReference bibleReference)
        {
            _ = int.TryParse(chapterMarker.Number, out var chapterNumber);
            var bibleChapter = new BibleChapter
            {
                Id = chapterNumber
            };
            if (!string.IsNullOrEmpty(chapterMarker.StartId) &&
                chapterMarker.StartId.Split(' ').LastOrDefault() is string chapter)
            {
                bibleChapter.Reference = new BibleReference(bibleReference) { Reference = chapter };
            }
            else
            {
                var chapterReference = chapterMarker.Number;
                bibleChapter.Reference = new BibleReference(bibleReference) { Reference = chapterReference };
            }
            return bibleChapter;
        }

        internal static IEnumerable<BibleVerse> ToBibleFormat(this UsxContent paragraph, BibleReference bibleReference, StringBuilder stringBuilder)
        {
            if (paragraph == null || paragraph.Content == null)
            {
                yield break;
            }

            var bibleVerse = new BibleVerse();
            foreach (var item in paragraph.Content)
            {
                if (item is UsxHeading textValue)
                {
                    stringBuilder.Append(textValue.Text);
                }
                else if (item is UsxMarker verseMarker && verseMarker.Style == "v")
                {
                    if (!string.IsNullOrEmpty(verseMarker.Number))
                    {
                        _ = int.TryParse(verseMarker.Number, out var verseNumber);
                        bibleVerse.Id = verseNumber;

                        if (!string.IsNullOrEmpty(verseMarker.StartId) &&
                            verseMarker.StartId.Split(' ').LastOrDefault() is string chapterVerse)
                        {
                            bibleVerse.Reference = new BibleReference(bibleReference) { Reference = chapterVerse };
                        }
                        else
                        {
                            var verseReference = verseMarker.Number.ToString(CultureInfo.InvariantCulture);
                            bibleVerse.Reference = new BibleReference(bibleReference) { Reference = verseReference };
                        }
                    }

                    bibleVerse.Text = stringBuilder.ToString();

                    if (!string.IsNullOrEmpty(bibleVerse.Text))
                    {
                        yield return bibleVerse;

                        stringBuilder.Clear();
                        bibleVerse = new BibleVerse();
                    }
                }
                else if (item is UsxContent usx && usx.Content != null)
                {
                    foreach (var verse in usx.ToBibleFormat(bibleReference, stringBuilder))
                    {
                        bibleVerse.Text += verse.Text;
                    }
                }
                else if (item is string text)
                {
                    bibleVerse.Text += text;
                }
            }
            yield return bibleVerse;
        }
    }
}
