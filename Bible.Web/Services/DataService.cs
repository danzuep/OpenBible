using Bible.Core.Models;
using Bible.Data.Services;

namespace Bible.Web.Services
{
    public interface IDataService<T>
    {
        Task<T> LoadAsync(string translation = "KJV");
    }

    public class DataService : IDataService<IEnumerable<BibleBook>>
    {
        public async Task<IEnumerable<BibleBook>> LoadAsync(string translation = "KJV")
        {
            var books = await SerializerService.GetBibleBooksFromResourceAsync(translation);
            return books;
        }

        //public async Task<BibleChapter?> LoadAsync(string bookName = "John", int chapterNumber = 1, string translation = "KJV")
        //{
        //    var books = await SerializerService.GetBibleBooksFromResourceAsync(translation);
        //    var book = books.Where(b => b.Reference.BookName == bookName).FirstOrDefault();
        //    var chapter = book?.Chapters.Where(c => c.Id == chapterNumber).FirstOrDefault();
        //    return chapter;
        //}
    }
}
