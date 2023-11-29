using BibleApp.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace BibleApp
{
    public sealed partial class MainPage : ContentPage
    {
        private MainPageViewModel _viewModel => (MainPageViewModel)BindingContext;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = Ioc.Default.GetRequiredService<MainPageViewModel>();
            //bibleChapterPicker.Focus();
        }

        private void OnTranslationSelectionChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && picker.SelectedItem is string selectedTranslation)
                System.Diagnostics.Debug.Assert(selectedTranslation == _viewModel.SelectedTranslation);
            _viewModel.GetTranslation(_viewModel.SelectedTranslation);
            bibleBookPicker.SelectedIndex = 0;
        }

        private void OnBookSelectionChanged(object sender, EventArgs e)
        {
            if (bibleBookPicker.SelectedIndex >= 0)
            {
                bibleChapterPicker.ItemsSource = _viewModel.SelectedBook?.ChapterNumbers;
                bibleChapterPicker.SelectedIndex = 0;
            }
        }

        private void OnChapterSelectionChanged(object sender, EventArgs e)
        {
            if (bibleChapterPicker.SelectedIndex >= 0 && _viewModel.Chapter != null)
            {
                //if (_viewModel.SelectedChapter == null)
                //    _viewModel.SelectedChapter = new();
                //_viewModel.SelectedChapter.Verses.Clear();
                //foreach (var verse in _viewModel.Chapter.Verses)
                //    _viewModel.SelectedChapter.Verses.Add(verse);
                _viewModel.SelectedChapter = _viewModel.Chapter;
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
