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

        public static BibleUiModel GetBible(BibleModel? bible, string translation, bool addChapters = true)
        {
            var bibleModel = new BibleUiModel(translation);
            if (bible != null)
            {
                foreach (var book in bible.Books)
                {
                    var bibleBook = new BookUiModel(book.Id, book.Reference.BookName, book.ChapterCount) { Copyright = translation };
                    if (addChapters)
                    {
                        foreach (var chapter in book.Chapters)
                        {
                            var bibleChapter = new ChapterUiModel(chapter.Id);
                            foreach (var verse in chapter.Verses)
                            {
                                var bibleVerse = new VerseUiModel(verse.Id, verse.Text);
                                bibleChapter.Add(bibleVerse);
                            }
                            bibleBook.Add(bibleChapter);
                        }
                    }
                    bibleModel.Add(bibleBook);
                }
            }
            return bibleModel;
        }
    }
}
