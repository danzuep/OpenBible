using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Bible.App.Models
{
    [ObservableObject]
    public sealed partial class BookUiModel : List<ChapterUiModel>
    {
        public int Id { get; }

        public string Name { get; } = default!;

        public string? Copyright { get; set; }

        public ObservableCollection<int> ChapterNumbers { get; }

        public BookUiModel(int id, string name, int chapterCount) : base(new List<ChapterUiModel>())
        {
            Id = id;
            Name = name;
            ChapterNumbers = new(Enumerable.Range(1, chapterCount));
        }

        public BookUiModel(List<ChapterUiModel> chapters, int id, string name) : base(chapters)
        {
            Id = id;
            Name = name;
            ChapterNumbers = new(Enumerable.Range(1, chapters.Count));
        }
        
        public override string ToString() =>
            $"Book #{Id}, {Name} ({this.Count} chapters)";
    }
}