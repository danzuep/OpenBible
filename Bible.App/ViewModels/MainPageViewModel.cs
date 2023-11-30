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
        private int translationIndex = -1;

        [ObservableProperty]
        private int bookIndex = -1;

        [ObservableProperty]
        private int chapterIndex = -1;

        private byte _bookIndexChecked => BookIndex < byte.MinValue ? byte.MinValue : (byte)BookIndex;
        private byte _chapterIndexChecked => ChapterIndex < byte.MinValue ? byte.MinValue : (byte)ChapterIndex;

        private readonly IDataService<BibleUiModel> _readerService;

        public MainPageViewModel(IDataService<BibleUiModel> readerService)
        {
            _readerService = readerService;
        }

        [RelayCommand]
        private void TranslationSelected(object? value)
        {
            Bible = _readerService.Load(Translations[TranslationIndex]);
            BookIndex = 0;
        }

        [RelayCommand]
        private void BookSelected(object? value)
        {
            ChapterIndex = 0;
        }

        [RelayCommand]
        private void ChapterSelected(object? value)
        {
            SelectedChapter = Bible?.Books[_bookIndexChecked].Chapters[_chapterIndexChecked];
        }

        internal ChapterUiModel? Chapter =>
            _readerService is BibleUiData reader ?
            reader.GetChapter(_bookIndexChecked, _chapterIndexChecked) :
            Bible?.Books[_bookIndexChecked].Chapters[_chapterIndexChecked];
    }
}