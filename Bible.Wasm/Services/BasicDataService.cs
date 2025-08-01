using Bible.Core.Models;
using Bible.Data.Services;

namespace Bible.Wasm.Services
{
    public interface ILoaderService<T>
    {
        Task<T> LoadAsync(string bibleVersion);
        Task<BibleBook?> LoadBookAsync(string? bibleVersion, string? bookName);
        Task<BibleChapter?> LoadChapterAsync(string? bibleVersion, string? bookName, int chapterNumber);
    }

    public interface IBibleDataService : ILoaderService<BibleModel>
    {
    }

    public class BasicDataService : IBibleDataService
    {
        private BibleModel? _bible;

        public async Task<BibleModel> LoadAsync(string bibleVersion = "KJV")
        {
            if (_bible == null || !_bible.Information.Translation.Equals(bibleVersion, StringComparison.OrdinalIgnoreCase))
                _bible = await SerializerService.GetBibleFromResourceAsync(bibleVersion);
            return _bible;
        }

        public async Task<BibleBook?> LoadBookAsync(string? bibleVersion = "KJV", string? bookName = "John")
        {
            var bibleBooks = await LoadAsync().ConfigureAwait(false);
            var book = bibleBooks.Where(b => b.Reference.BookName == bookName || b.Aliases.Contains(bookName)).FirstOrDefault();
            return book;
        }

        public async Task<BibleChapter?> LoadChapterAsync(string? bibleVersion, string? bookName = "John", int chapterNumber = 1)
        {
            var bibleBooks = await LoadAsync().ConfigureAwait(false);
            var book = bibleBooks.Where(b => b.Reference.BookName == bookName || b.Aliases.Contains(bookName)).FirstOrDefault();
            var chapter = book?.Chapters.Where(c => c.Id == chapterNumber).FirstOrDefault();
            return chapter;
        }
    }
}
