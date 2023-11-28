using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BibleApp.Models
{
    public sealed partial class BookUiModel : ObservableObject
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private string name = default!;

        //[ObservableProperty]
        //private ObservableCollection<ChapterUiModel> chapters = new();

        public int ChapterCount { get; init; }

        public ObservableCollection<int> ChapterNumbers =>
            new(Enumerable.Range(1, ChapterCount));

        public override string ToString() =>
            $"Book #{Id}, {Name} ({ChapterCount} chapters)";
    }
}