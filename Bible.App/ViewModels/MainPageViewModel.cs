using Bible.Core.Models;
using Bible.Interfaces;
using BibleApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BibleApp.ViewModels
{
    public sealed partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private BibleUiModel? bible;

        [ObservableProperty]
        private BookUiModel? selectedBook;

        [ObservableProperty]
        private ChapterUiModel? selectedChapter;

        private const string _testUsxBook = "zho/OCCB/GEN";

        [ObservableProperty]
        private ObservableCollection<string> translations = [
            "eng/WEB",
            "eng/WEBBE",
            "eng/WEBME",
            "chi/CUV",
            "chi/CUVS",
            //_testUsxBook,
            "tha/KJVTHAI"];

        [ObservableProperty]
        private string selectedTranslation;

        [ObservableProperty]
        private int bookIndex;

        [ObservableProperty]
        private int chapterIndex;

        private byte _bookIndex => BookIndex < byte.MinValue ? byte.MinValue : (byte)BookIndex;
        private byte _chapterIndex => ChapterIndex < byte.MinValue ? byte.MinValue : (byte)ChapterIndex;

        private BibleModel? _bible;
        private readonly IBibleService _readerService;

        public MainPageViewModel(IBibleService readerService)
        {
            _readerService = readerService;
            SelectedTranslation = Translations[0];
        }

        internal void GetTranslation(string? translation)
        {
            //ArgumentNullException.ThrowIfNull(translation);
            translation ??= SelectedTranslation;
            Bible = new() { Translation = translation };
            _bible = _readerService.LoadBible(translation);
            if (_bible != null)
            {
                foreach (var book in _bible.Books)
                {
                    var bibleBook = new BookUiModel
                    {
                        Id = book.Id,
                        Name = book.Reference.BookName,
                        ChapterCount = book.ChapterCount
                    };
                    Bible?.Books.Add(bibleBook);
                }
            }
        }

        private ChapterUiModel GetChapter(byte bookIndex, byte chapterIndex)
        {
            var bibleChapter = new ChapterUiModel { Id = chapterIndex + 1 };
            if (_bible != null)
            {
                foreach (var verse in _bible.Books[bookIndex].Chapters[chapterIndex].Verses)
                {
                    var bibleVerse = new VerseUiModel { Id = verse.Id, Text = verse.Text };
                    bibleChapter.Verses.Add(bibleVerse);
                }
            }
            return bibleChapter;
        }

        internal ChapterUiModel GetChapter() => GetChapter(_bookIndex, _chapterIndex);
    }
}