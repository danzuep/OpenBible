using Bible.Core.Models;
using Bible.Interfaces;
using BibleApp.Models;

namespace Bible.Reader.Services
{
    public sealed class BibleUiData : IDataService<BibleUiModel>
    {
        private BibleModel? _bible = null;

        public BibleUiModel Load(string fileName, string suffix = ".xml")
        {
            _bible = new BibleReader().Load(fileName);
            var translation = Path.GetFileName(fileName);
            return GetBible(_bible, translation);
        }

        public BibleUiModel LoadMock(BibleModel bible, string translation)
        {
            var bibleModel = new BibleUiModel() { Translation = translation };
            if (bible != null)
            {
                foreach (var book in bible.Books)
                {
                    var bibleBook = new BookUiModel
                    {
                        Id = book.Id,
                        Name = book.Reference.BookName,
                        ChapterCount = book.ChapterCount
                    };
                    bibleModel.Books.Add(bibleBook);
                }
            }
            return bibleModel;
        }

        private static ChapterUiModel GetChapter(BibleModel? bible, byte bookIndex, byte chapterIndex)
        {
            var bibleChapter = new ChapterUiModel { Id = chapterIndex + 1 };
            if (bible != null)
            {
                foreach (var verse in bible.Books[bookIndex].Chapters[chapterIndex].Verses)
                {
                    var bibleVerse = new VerseUiModel { Id = verse.Id, Text = verse.Text };
                    bibleChapter.Verses.Add(bibleVerse);
                }
            }
            return bibleChapter;
        }

        public static ChapterUiModel? GetChapter(BibleUiModel? bible, byte bookIndex, byte chapterIndex) =>
            bible?.Books[bookIndex].Chapters[chapterIndex];

        public ChapterUiModel? GetChapter(byte bookIndex, byte chapterIndex) =>
            GetChapter(_bible, bookIndex, chapterIndex);

        public static BibleUiModel GetBible(BibleModel? bible, string translation)
        {
            var bibleModel = new BibleUiModel() { Translation = translation };
            if (bible != null)
            {
                foreach (var book in bible.Books)
                {
                    var bibleBook = new BookUiModel
                    {
                        Id = book.Id,
                        Name = book.Reference.BookName,
                        ChapterCount = book.ChapterCount
                    };
                    bibleModel.Books.Add(bibleBook);
                }
            }
            return bibleModel;
        }
    }
}
