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
            if (bibleChapterPicker.SelectedIndex >= 0)
            {
                _viewModel.SelectedChapter?.Verses.Clear();
                _viewModel.SelectedChapter = _viewModel.GetChapter();
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
