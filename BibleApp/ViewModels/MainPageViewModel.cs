using Bible.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BibleApp
{
    public sealed partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string[] translations = ["eng-WEB", "zho-CUV"];

        [ObservableProperty]
        private string selectedTranslation;

        [ObservableProperty]
        private IReadOnlyList<BibleBook>? bibleBooks;

        [ObservableProperty]
        private BibleBook? selectedBook;

        [ObservableProperty]
        private BibleChapter? selectedChapter;

        public MainPageViewModel()
        {
            SelectedTranslation = Translations[0];
        }
    }
}