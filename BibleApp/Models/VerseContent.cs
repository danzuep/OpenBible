using CommunityToolkit.Mvvm.ComponentModel;

namespace BibleApp.Models
{
    public sealed partial class VerseContent : ObservableObject
    {
        [ObservableProperty]
        private bool selected = false;

        [ObservableProperty]
        private int verseNumber = default!;

        [ObservableProperty]
        private string verseText = default!;
    }
}
