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

        //private const string _testUsxBook = "zho/OCCB/GEN";

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

        private readonly IUiDataService _readerService;

        public MainPageViewModel(IUiDataService readerService)
        {
            _readerService = readerService;
        }

        [RelayCommand]
        private async Task TranslationSelectedAsync(object? value)
        {
            var fileName = Translations[TranslationIndex];
            Bible = await _readerService.LoadFileAsync(fileName);
            BookIndex = 0;
        }
    }
}