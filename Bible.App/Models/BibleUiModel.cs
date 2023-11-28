using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BibleApp.Models
{
    public sealed partial class BibleUiModel : ObservableObject
    {
        [ObservableProperty]
        private string translation = default!;

        [ObservableProperty]
        private ObservableCollection<BookUiModel> books = new();

        public override string ToString() =>
            $"Bible: {Translation} ({Books.Count} books)";
    }
}