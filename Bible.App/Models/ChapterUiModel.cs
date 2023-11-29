using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BibleApp.Models
{
    public sealed partial class ChapterUiModel : ObservableObject
    {
        //[ObservableProperty]
        //private int id;

        //[ObservableProperty]
        //private IList<VerseUiModel> verses = new List<VerseUiModel>();

        public int Id { get; set; }

        public ObservableCollection<VerseUiModel> Verses { get; set; } = new();

        public override string ToString() =>
            $"Chapter {Id} ({Verses.Count} verses)";
    }
}