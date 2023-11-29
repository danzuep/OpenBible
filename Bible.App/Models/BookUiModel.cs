using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BibleApp.Models
{
    public sealed partial class BookUiModel : ObservableObject
    {
        //[ObservableProperty]
        //private int id;

        //[ObservableProperty]
        //private string name = default!;

        //[ObservableProperty]
        //private IList<ChapterUiModel> chapters = new List<ChapterUiModel>();

        public int Id { get; set; }

        public string Name { get; set; } = default!;

        public ObservableCollection<ChapterUiModel> Chapters { get; set; } = new();


        private int _chapterCount = 1;
        public int ChapterCount
        {
            get => _chapterCount;
            init
            {
                if (SetProperty(ref _chapterCount, value))
                {
                    _chapterNumbers = new(ParallelEnumerable.Range(1, _chapterCount));
                    OnPropertyChanged(nameof(ChapterNumbers));
                }
            }
        }

        Lazy<IEnumerable<int>>? _chapterNumbers;

        public ObservableCollection<int> ChapterNumbers =>
            new(_chapterNumbers?.Value ?? [1]);

        public override string ToString() =>
            $"Book #{Id}, {Name} ({ChapterCount} chapters)";
    }
}