using Bible.Core.Models;
using Bible.Data.Services;

namespace Bible.Web.Services
{
    public interface IDataService<T>
    {
        Task<T> LoadAsync(string translation = "KJV");
        BibleChapter? LoadChapter(string? bookName = "John", int chapterNumber = 1);
    }

    public class DataService : IDataService<IEnumerable<BibleBook>>
    {
        private BibleModel? _bible;

        public async Task<IEnumerable<BibleBook>> LoadAsync(string translation = "KJV")
        {
            if (_bible == null)
                _bible = await SerializerService.GetBibleFromResourceAsync(translation);
            return _bible.Books;
        }

        public BibleChapter? LoadChapter(string? bookName = "John", int chapterNumber = 1)
        {
            var bibleBooks = LoadAsync().GetAwaiter().GetResult();
            var book = bibleBooks.Where(b => b.Reference.BookName == bookName || b.Aliases.Contains(bookName)).FirstOrDefault();
            var chapter = book?.Chapters.Where(c => c.Id == chapterNumber).FirstOrDefault();
            return chapter;
        }
    }
}
