using Bible.Core.Models;
using Bible.Reader.Extensions;
using Bible.Reader.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Bible.Reader.Adapters
{
    public static class UsxAdapter
    {
        public static BibleModel ToBibleFormat(this XmlUsx xmlBible, string language = null, string translation = null)
        {
            var bibleReference = new BibleReference
            {
                Translation = translation ?? xmlBible.Version.ToString(),
            };
            var bible = new BibleModel
            {
                Information = new BibleInformation()
                {
                    //Name = xmlBible.Items.OfType<XmlUsx3Chapter>()?.Value,
                    //Translation = xmlBible.Version?.Trim(),
                    IsoLanguage = language
                },
                Books = xmlBible.Items.OfType<XmlUsxBook>().Select(book =>
                {
                    var isBook = Enum.TryParse(book.Id, out BibleBookId bibleBookId);
                    var bibleBookName = "Genesis"; //bibleBookId.GetDescription();
                    var bookReference = new BibleReference(bibleReference) { BookName = bibleBookName };
                    var bibleBook = new BibleBook
                    {
                        Id = (int)bibleBookId,
                        Reference = bookReference,
                        Aliases = Array.Empty<string>(),
                        Chapters = xmlBible.Items.OfType<XmlUsxChapter>().Select(chapter =>
                        {
                            var chapterNumber = chapter.Number.ToString(CultureInfo.InvariantCulture);
                            var chapterReference = new BibleReference(bookReference) { Reference = chapterNumber };
                            //var paragraphs = chapter.Items.OfType<XmlUsxParagraph>().ToList();
                            var verseParagraphs = chapter.Paragraphs.Where(p => p.Style?.Equals("p") ?? false);
                            var verses = new List<BibleVerse>();
                            foreach (var paragraph in verseParagraphs)
                            {
                                var verseVerses = paragraph.Items?.OfType<XmlUsxVerse>();
                                if (verseVerses != null)
                                {
                                    foreach (var verse in verseVerses)
                                    {
                                        if (verse != null)
                                        {
                                            var bibleVerse = new BibleVerse
                                            {
                                                Reference = chapterReference,
                                                Text = verse.VerseText
                                            };
                                            if (int.TryParse(verse.Number, out int verseNumber))
                                                bibleVerse.Id = verseNumber;
                                            verses.Add(bibleVerse);
                                        }
                                    }
                                }
                            }
                            var bibleChapter = new BibleChapter
                            {
                                Id = chapter.Number,
                                Reference = chapterReference,
                                Verses = verses
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
