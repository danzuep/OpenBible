using Bible.Core.Models;
using Bible.Reader.Extensions;
using Bible.Reader.Models;
using System;
using System.Globalization;
using System.Linq;

namespace Bible.Reader.Adapters
{
    public static class Usx3Adapter
    {
        public static BibleModel ToBibleFormat(this XmlUsx3 xmlBible, string language = null, string translation = null)
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
                Books = xmlBible.Items.OfType<XmlUsx3Book>().Select(book =>
                {
                    var isBook = Enum.TryParse(book.Id, out BibleBookId bibleBookId);
                    var bibleBookName = "Genesis"; //bibleBookId.GetDescription();
                    var bookReference = new BibleReference(bibleReference) { BookName = bibleBookName };
                    var paragraphs = xmlBible.Items.OfType<XmlUsx3Paragraph>();
                    var verseParagraphs = paragraphs.Where(p => p.Style.Equals("p"));
                    var verseVerses = verseParagraphs.SelectMany(p => p.Items.Where(p => p.Style.Equals("v"))).FirstOrDefault();
                    var bibleBook = new BibleBook
                    {
                        BookNumber = (int)bibleBookId,
                        Reference = bookReference,
                        Aliases = Array.Empty<string>(),
                        Chapters = xmlBible.Items.OfType<XmlUsx3Chapter>().Select(chapter =>
                        {
                            var chapterNumber = chapter.Number.ToString(CultureInfo.InvariantCulture);
                            var chapterReference = new BibleReference(bookReference) { Reference = chapterNumber };
                            var bibleChapter = new BibleChapter
                            {
                                ChapterNumber = chapter.Number,
                                Reference = chapterReference,
                                //Verses = paragraphs.Where(p => p.S).Items.OfType<XmlUsx3Verse>()
                                //    .Select(verse => new BibleVerse
                                //    {
                                //        Number = verse.Number,
                                //        Reference = chapterReference,
                                //        Text = verse.Text?.Trim(new char[] { '\r', '\n' })
                                //    }).ToArray()
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
