using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Bible.App.Models
{
    public sealed partial class BookUiModel : List<ChapterUiModel>
    {
        public BookUiModel(int id, string name, int chapterCount, List<ChapterUiModel>? chapters = null) : base(chapters ?? new())
        {
            Id = id;
            Name = name;
            ChapterCount = chapterCount;
            ChapterNumbers = new(Enumerable.Range(1, chapterCount));
        }

        public int Id { get; }

        public string Name { get; } = default!;

        public string? Copyright { get; set; }

        public int ChapterCount { get; }

        public ObservableCollection<int> ChapterNumbers { get; }

        public override string ToString() =>
            $"Book #{Id}, {Name} ({ChapterCount} chapters)";
    }
}