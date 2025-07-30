using System;
using Bible.Backend.Abstractions;
using Bible.Backend.Services;
using Bible.Core.Abstractions;
using Bible.Core.Models;

namespace Bible.Web.Services
{
    public class DataService : IBibleDataService
    {
        private BibleModel? _bible;
        private readonly IDataService<BibleModel> _dataService;
        private readonly IParser<BibleBook> _parser;

        public DataService(IDataService<BibleModel> dataService, IParser<BibleBook> parser)
        {
            _dataService = dataService;
            _parser = parser;
        }

        public Task<BibleModel> LoadAsync(string version = "eng-WEBBE")
        {
            _bible ??= Load(version);
            return Task.FromResult(_bible);
        }

        private BibleBook? LoadBook(string? bookCode)
        {
            _bible ??= Load();
            var book = _bible?.FirstOrDefault(b =>
                b.Reference.BookCode.Equals(bookCode, StringComparison.OrdinalIgnoreCase) ||
                b.Reference.BookName.Equals(bookCode, StringComparison.OrdinalIgnoreCase) ||
                b.Aliases.Contains(bookCode, StringComparer.OrdinalIgnoreCase));
            return book;
        }

        public async Task<BibleBook?> LoadBookAsync(string? bibleVersion, string? bookCode)
        {
            if (string.IsNullOrEmpty(bibleVersion) || string.IsNullOrEmpty(bookCode)) return null;
            var bible = await _parser.ParseAsync(bibleVersion, bookCode);
            return bible;
        }

        public BibleChapter? LoadChapter(string? bookName = "JHN", int chapterNumber = 1)
        {
            _bible ??= Load();
            var bibleBooks = _bible.Books;
            var book = bibleBooks.Where(b => b.Reference.BookName == bookName || b.Aliases.Contains(bookName)).FirstOrDefault();
            var chapter = book?.Chapters.Where(c => c.Id == chapterNumber).FirstOrDefault();
            return chapter;
        }

        private BibleModel Load(string version = "eng-WEBBE")
        {
            return _dataService.Load(version);
        }
    }
}
