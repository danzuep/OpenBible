using Bible.Interfaces;
using BibleApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BibleApp.ViewModels
{
    public sealed partial class MainPageViewModel : ObservableObject
    {
        private int _chapterCount = 150;
        public ObservableCollection<int> ChapterNumbers { get; }

        [ObservableProperty]
        private int chapterIndex;

        public IReadOnlyList<VerseUiModel> Verses { get; private set; }

        private readonly IDataService<VerseUiModel> _readerService;

        public MainPageViewModel(IDataService<VerseUiModel> readerService)
        {
            _readerService = readerService;
            ChapterNumbers = new(Enumerable.Range(1, _chapterCount));
            ChapterIndex = 0;
            Verses = _readerService.LoadVerses(1);
        }

        [RelayCommand]
        private void ChapterSelected(int? chapterNumber)
        {
            Verses = _readerService.LoadVerses(ChapterIndex + 1);
            OnPropertyChanged(nameof(Verses));
        }
    }
}