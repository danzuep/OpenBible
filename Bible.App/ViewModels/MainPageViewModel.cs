using Bible.Core.Models;
using Bible.Interfaces;
using Bible.Reader.Services;
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

        public ObservableCollection<string> Translations { get; } = [
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

        private readonly IDataService<BibleUiModel> _readerService;

        public MainPageViewModel(IDataService<BibleUiModel> readerService)
        {
            _readerService = readerService;
            SelectedTranslation = Translations[0];
        }

        internal void GetTranslation(string? translation)
        {
            //ArgumentNullException.ThrowIfNull(translation);
            translation ??= SelectedTranslation;
            Bible = _readerService.Load(translation);
        }

        internal ChapterUiModel? Chapter =>
            _readerService is BibleUiData reader ?
            reader.GetChapter(_bookIndex, _chapterIndex) :
            Bible?.Books[_bookIndex].Chapters[_chapterIndex];
    }
}