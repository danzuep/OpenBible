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
                System.Diagnostics.Debug.Assert(selectedTranslation == _viewModel.SelectedTranslation);
            _viewModel.LoadBibleBooks();
            bibleBookPicker.SelectedIndex = 0;
        }

        private void OnBookSelectionChanged(object sender, EventArgs e)
        {
            bibleChapterPicker.ItemsSource = _viewModel.SelectedBook?.ChapterNumbers;
            bibleChapterPicker.SelectedIndex = 0;
        }

        private void OnChapterSelectionChanged(object sender, EventArgs e)
        {
            if (bibleChapterPicker.SelectedIndex >= 0)
            {
                _viewModel.SelectedChapter?.Verses.Clear();
                _viewModel.SelectedChapter = _viewModel.LoadChapter();
            }
        }

        private void OnSwipeLeft(object sender, SwipedEventArgs e)
        {
            if (_viewModel.SelectedBook?.ChapterCount > bibleChapterPicker.SelectedIndex + 1)
                bibleChapterPicker.SelectedIndex++;
        }

        private void OnSwipeRight(object sender, SwipedEventArgs e)
        {
            if (bibleChapterPicker.SelectedIndex > 0)
                bibleChapterPicker.SelectedIndex--;
        }
    }
}
