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

        public IReadOnlyList<string> Verses { get; private set; }

        public MainPageViewModel()
        {
            ChapterNumbers = new(Enumerable.Range(1, _chapterCount));
            ChapterIndex = 0;
            Verses = LoadVerses(1);
        }

        [RelayCommand]
        private void ChapterSelected(int? chapterNumber)
        {
            Verses = LoadVerses(ChapterIndex + 1);
            OnPropertyChanged(nameof(Verses));
        }

        private IReadOnlyList<string> LoadVerses(int chapterNumber)
        {
            int verseCount = Random.Shared.Next(2, 176);
            var verses = Enumerable.Range(1, verseCount)
                .Select(v => $"Chapter #{chapterNumber}, Verse #{v}.");
            return verses.ToArray();
        }
    }
}