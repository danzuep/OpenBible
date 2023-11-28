using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BibleApp.Models
{
    public sealed partial class ChapterUiModel : ObservableObject
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private ObservableCollection<VerseUiModel> verses = new();

        public override string ToString() =>
            $"Chapter {Id} ({Verses.Count} verses)";
    }
}