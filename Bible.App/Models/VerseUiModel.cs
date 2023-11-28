using CommunityToolkit.Mvvm.ComponentModel;

namespace BibleApp.Models
{
    public sealed partial class VerseUiModel : ObservableObject
    {
        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private string text = default!;

        public override string ToString() =>
            $"{Id} {Text}";
    }
}