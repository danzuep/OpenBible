using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BibleApp.Models
{
    public sealed partial class BibleUiModel : ObservableObject
    {
        //[ObservableProperty]
        //private string translation = default!;

        //[ObservableProperty]
        //private IList<BookUiModel> books = new List<BookUiModel>();

        public string Translation { get; set; } = default!;

        public ObservableCollection<BookUiModel> Books { get; set; } = new();

        public override string ToString() =>
            $"Bible: {Translation} ({Books.Count} books)";
    }
}