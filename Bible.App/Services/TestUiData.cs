using Bible.App.Models;
using Bible.App.Abstractions;

namespace Bible.Reader.Services
{
    public sealed class TestUiData : IUiDataService
    {
        public BibleUiModel Load(string fileName, string suffix = ".xml") =>
            LoadTest(66, suffix.Length * 3, fileName.Length * 3);

        public async Task<BibleUiModel> LoadAsync(string fileName)
        {
            await Task.CompletedTask;
            return Load(fileName);
        }

        public BibleUiModel LoadTest(int bookCount, int chapterCount, int verseCount, string name = "Books, Chapters, and Verses")
        {
            var books = Enumerable.Range(1, bookCount);
            var chapters = Enumerable.Range(1, chapterCount);
            var verses = Enumerable.Range(1, verseCount);

            var bible = new BibleUiModel(name);
            foreach (var bookId in books)
            {
                var bibleBook = new BookUiModel(bookId, $"Book #{bookId}", chapterCount);
                foreach (var chapterId in chapters)
                {
                    var bibleChapter = new ChapterUiModel(chapterId);
                    foreach (var verseId in verses)
                    {
                        var bibleVerse = new VerseUiModel(verseId, $"Book #{bookId}, Chapter #{chapterId}, Verse #{verseId}.");
                        bibleChapter.Add(bibleVerse);
                    }
                    bibleBook.Add(bibleChapter);
                }
                bible.Add(bibleBook);
            }
            return bible;
        }
    }
}
