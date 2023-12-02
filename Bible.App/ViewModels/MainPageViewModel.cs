using Bible.Core.Abstractions;
using Bible.Reader.Services;
using Bible.App.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Bible.App.Abstractions;

namespace Bible.App.ViewModels
{
    public sealed partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private BibleUiModel? bible;

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

        private int _chapterIndex = -1;
        public int ChapterIndex
        {
            get => _chapterIndex;
            set => SetProperty(ref _chapterIndex, value);
        }

        private readonly IUiDataService _readerService;

        public MainPageViewModel(IUiDataService readerService)
        {
            _readerService = readerService;
        }

        [RelayCommand]
        private async Task TranslationSelectedAsync(object? value)
        {
            var fileName = Translations[TranslationIndex];
            Bible = await _readerService.LoadAsync(fileName);
            //Bible = _readerService.Load(fileName);
            BookIndex = 0;
        }
    }
}