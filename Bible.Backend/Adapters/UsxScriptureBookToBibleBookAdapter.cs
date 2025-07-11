﻿using System.Globalization;
using System.Text;
using Bible.Backend.Models;
using Bible.Core.Models;

namespace Bible.Backend.Adapters
{
    public static class UsxScriptureBookToBibleBookAdapter
    {
        public static BibleBook? ToBibleFormat(this UsxScriptureBook? book)
        {
            if (book == null)
            {
                return null;
            }

            var bibleReference = new BibleReference
            {
                BookName = book.Translation.BookCode
            };

            var bibleBook = new BibleBook { Reference = bibleReference };
            var chapters = new List<BibleChapter>();
            var bibleChapter = new BibleChapter();
            var verses = new List<BibleVerse>();
            var stringBuilder = new StringBuilder();

            foreach (var content in book.Content)
            {
                if (content is UsxPara heading && heading.Style == "h" &&
                    heading.Content.FirstOrDefault() is string bookName)
                {
                    bibleReference.BookName = bookName;
                    //bibleBook.Reference = bibleReference;
                }
                else if (content is UsxMarker chapterMarker)
                {
                    if (chapterMarker.Style == "c" && chapterMarker.Number != 0)
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
            var bibleChapter = new BibleChapter
            {
                Id = chapterMarker.Number
            };
            if (!string.IsNullOrEmpty(chapterMarker.StartId) &&
                chapterMarker.StartId.Split(' ').LastOrDefault() is string chapter)
            {
                bibleChapter.Reference = new BibleReference(bibleReference) { Reference = chapter };
            }
            else
            {
                var chapterReference = chapterMarker.Number.ToString(CultureInfo.InvariantCulture);
                bibleChapter.Reference = new BibleReference(bibleReference) { Reference = chapterReference };
            }
            return bibleChapter;
        }

        internal static IEnumerable<BibleVerse> ToBibleFormat(this UsxPara paragraph, BibleReference bibleReference, StringBuilder stringBuilder)
        {
            if (paragraph == null || paragraph.Style != "p")
            {
                yield break;
            }

            var bibleVerse = new BibleVerse();
            foreach (var item in paragraph.Content)
            {
                if (item is string textValue)
                {
                    stringBuilder.Append(textValue);
                }
                else if (item is IUsxTextBase value)
                {
                    stringBuilder.Append(value.Text);
                }
                else if (item is UsxMarker verseMarker && verseMarker.Style == "v")
                {
                    if (verseMarker.Number != 0)
                    {
                        bibleVerse.Id = verseMarker.Number;

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
            }
        }
    }
}
