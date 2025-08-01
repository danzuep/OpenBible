using Bible.Backend.Abstractions;
using Bible.Core.Abstractions;
using Bible.Core.Models;
using Bible.Data.Services;

namespace Bible.Wasm.Services
{
    public class DataService : IBibleDataService
    {
        private BibleModel? _bible;
        private IBibleDataService? _kjvDataService;
        private readonly IDataService<BibleModel> _dataService;
        private readonly IParser<BibleBook> _parser;

        public DataService()
        {
            _dataService = Program.GetRequiredService<IDataService<BibleModel>>();
            _parser = Program.GetRequiredService<IParser<BibleBook>>();
        }

        public async Task<BibleModel> LoadAsync(string bibleVersion = "eng-WEBBE")
        {
            if (bibleVersion == "KJV")
            {
                _kjvDataService ??= new BasicDataService();
                _bible = await _kjvDataService.LoadAsync("KJV");
                return _bible;
            }
            _bible ??= _dataService.Load(bibleVersion);
            return _bible;
        }

        private async Task<BibleModel> InternalLoadAsync(string bibleVersion = "eng-WEBBE")
        {
            if (_bible == null || !_bible.Information.Translation.Equals(bibleVersion, StringComparison.OrdinalIgnoreCase))
                _bible = await SerializerService.GetBibleFromResourceAsync(bibleVersion);
            return _bible;
        }

        public async Task<BibleBook?> LoadBookAsync(string? bibleVersion, string? bookCode)
        {
            if (bibleVersion == "KJV")
            {
                _kjvDataService ??= new BasicDataService();
                var book = await _kjvDataService.LoadBookAsync("KJV", bookCode);
                return book;
            }
            if (string.IsNullOrEmpty(bibleVersion) || string.IsNullOrEmpty(bookCode)) return null;
            var bible = await _parser.ParseAsync(bibleVersion, bookCode);
            return bible;
        }

        public async Task<BibleChapter?> LoadChapterAsync(string? bibleVersion, string? bookCode = "JHN", int chapterNumber = 1)
        {
            var bibleBook = _bible != null ?
                LoadBook(_bible, bookCode) :
                await LoadBookAsync("eng-WEBBE", bookCode).ConfigureAwait(false);
            var chapter = bibleBook?.Chapters.Where(c => c.Id == chapterNumber).FirstOrDefault();
            return chapter;
        }

        private BibleBook? LoadBook(BibleModel? _bible, string? bookCode)
        {
            _bible ??= _dataService.Load(_bible.Information.Translation);
            var book = _bible?.FirstOrDefault(b =>
                b.Reference.BookCode.Equals(bookCode, StringComparison.OrdinalIgnoreCase) ||
                b.Reference.BookName.Equals(bookCode, StringComparison.OrdinalIgnoreCase) ||
                b.Aliases.Contains(bookCode, StringComparer.OrdinalIgnoreCase));
            return book;
        }
    }
}
