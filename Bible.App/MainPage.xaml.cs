using BibleApp.ViewModels;

namespace BibleApp
{
    public partial class MainPage : ContentPage
    {
        readonly MainPageViewModel _viewModel = new();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        private void OnTranslationSelectionChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && picker.SelectedItem is string selectedTranslation)
            {
                if (selectedTranslation != _viewModel.SelectedTranslation)
                    System.Diagnostics.Debugger.Break();
                _viewModel.LoadSelectedBible(selectedTranslation);
                //bibleBookPicker.ItemsSource = _viewModel._bible.Books;
                bibleBookPicker.SelectedIndex = 0;
            }
        }

        private void OnBookSelectionChanged(object sender, EventArgs e)
        {
            //bibleChapterPicker.ItemsSource = _viewModel._bibleBook.ChapterNumbers;
            bibleChapterPicker.SelectedIndex = 0;
        }

        private void OnChapterSelectionChanged(object sender, EventArgs e)
        {
            //_viewModel.BibleVerses = _viewModel._bibleChapter.Verses;
        }

        private void OnSwipeLeft(object sender, SwipedEventArgs e)
        {
            if (_viewModel._bibleBook.Chapters.Count > bibleChapterPicker.SelectedIndex + 1)
                bibleChapterPicker.SelectedIndex++;
        }

        private void OnSwipeRight(object sender, SwipedEventArgs e)
        {
            if (bibleChapterPicker.SelectedIndex > 0)
                bibleChapterPicker.SelectedIndex--;
        }
    }
}
