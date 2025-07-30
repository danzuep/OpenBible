using Bible.Core.Models;
using Bible.Data.Services;

namespace Bible.Web.Services
{
    public interface ILoaderService<T>
    {
        Task<T> LoadAsync(string translation = "KJV");
        Task<BibleBook?> LoadBookAsync(string? bibleVersion, string? bookName = "JHN");
        BibleChapter? LoadChapter(string? bookName = "JHN", int chapterNumber = 1);
    }

    public interface IBibleDataService : ILoaderService<BibleModel> { }

    public class BasicDataService : IBibleDataService
    {
        private BibleModel? _bible;

        public async Task<BibleModel> LoadAsync(string translation = "KJV")
        {
            if (_bible == null || !_bible.Information.Translation.Equals(translation, StringComparison.OrdinalIgnoreCase))
                _bible = await SerializerService.GetBibleFromResourceAsync(translation);
            return _bible;
        }

        public async Task<BibleBook?> LoadBookAsync(string? bibleVersion, string? bookName = "JHN")
        {
            var bibleBooks = await LoadAsync().ConfigureAwait(false);
            var book = bibleBooks.Where(b => b.Reference.BookName == bookName || b.Aliases.Contains(bookName)).FirstOrDefault();
            return book;
        }

        public BibleChapter? LoadChapter(string? bookName = "JHN", int chapterNumber = 1)
        {
            var bibleBooks = LoadAsync().GetAwaiter().GetResult();
            var book = bibleBooks.Where(b => b.Reference.BookName == bookName || b.Aliases.Contains(bookName)).FirstOrDefault();
            var chapter = book?.Chapters.Where(c => c.Id == chapterNumber).FirstOrDefault();
            return chapter;
        }
    }
}
