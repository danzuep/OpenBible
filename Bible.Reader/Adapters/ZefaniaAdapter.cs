using Bible.Core.Models;
using Bible.Reader.Models;
using System;
using System.Globalization;
using System.Linq;

namespace Bible.Reader.Adapters
{
    public static class ZefaniaAdapter
    {
        public static BibleModel ToBibleFormat(this XmlZefania05 xmlBible, string language = null, string translation = null)
        {
            var bibleReference = new BibleReference { Translation = translation };
            bool isDate = DateTime.TryParse(xmlBible.Information?.Date, out DateTime date);
            var bible = new BibleModel
            {
                Information = new BibleInformation()
                {
                    Name = xmlBible.BibleName?.Trim(),
                    Translation = translation,
                    Version = xmlBible.Version?.Trim(),
                    IsoLanguage = language ?? xmlBible.Information?.Language?.Trim(),
                    PublishedYear = isDate ? date.Year : default,
                },
                Books = xmlBible.BibleBooks.Select(book =>
                {
                    var bookReference = new BibleReference(bibleReference) { BookName = book.Name };
                    var bibleBook = new BibleBook
                    {
                        Id = book.Number,
                        Reference = bookReference,
                        Aliases = Array.Empty<string>(),
                        Chapters = book.Chapters.Select(chapter =>
                        {
                            var chapterNumber = chapter.Number.ToString(CultureInfo.InvariantCulture);
                            var chapterReference = new BibleReference(bookReference) { Reference = chapterNumber };
                            var bibleChapter = new BibleChapter
                            {
                                Id = chapter.Number,
                                Reference = chapterReference,
                                Verses = chapter.Verses
                                    .Select(verse => new BibleVerse
                                    {
                                        Id = verse.Number,
                                        Reference = chapterReference,
                                        Text = verse.Text?.Trim(new char[] { '\r', '\n' })
                                    }).ToArray()
                            };
                            return bibleChapter;
                        }).ToArray()
                    };
                    return bibleBook;
                }).ToArray()
            };
            return bible;
        }
    }
}
