using Bible.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BibleApp.ViewModels
{
    public sealed partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string[] translations = [
            //"zho/OCCB/GEN",
            "chi/CUV",
            "chi/CUVS",
            "tha/KJVTHAI",
            "eng/WEB",
            "eng/WEBBE",
            "eng/WEBME"];

        [ObservableProperty]
        private string selectedTranslation;

        [ObservableProperty]
        private IReadOnlyList<BibleBook>? bibleBooks;

        [ObservableProperty]
        private BibleBook? selectedBook;

        [ObservableProperty]
        private BibleChapter? selectedChapter;

        [RelayCommand]
        private void OnLabelTapped(object sender)
        {
            if (sender is Label label)
            {
                var verseText = label.Text;
            }
        }

        public MainPageViewModel()
        {
            SelectedTranslation = Translations[0];
        }
    }
}