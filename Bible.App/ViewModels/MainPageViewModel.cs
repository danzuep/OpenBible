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
    }
}