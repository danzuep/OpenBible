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

        //[ObservableProperty]
        //private int selectedChapter;

        public ObservableCollection<VerseUiModel> Verses { get; }

        private readonly IDataService<VerseUiModel> _readerService;

        public MainPageViewModel(IDataService<VerseUiModel> readerService)
        {
            _readerService = readerService;
            ChapterNumbers = new(Enumerable.Range(1, _chapterCount));
            ChapterIndex = 0;
            Verses = new(_readerService.LoadVerses(1));
        }

        [RelayCommand]
        private void ChapterSelected(int? chapterNumber)
        {
            var verses = _readerService.LoadVerses(ChapterIndex + 1);
            Verses.Clear();
            foreach (var verse in verses)
            {
                Verses.Add(verse);
            }
        }
    }
}