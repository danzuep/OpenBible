using BibleApp.Models;
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
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = Ioc.Default.GetRequiredService<MainPageViewModel>();
            _viewModel.TranslationIndex = 0;
            bibleChapterPicker.Focus();
        }

        private void OnBookSelectionChanged(object sender, EventArgs e)
        {
            _viewModel.ChapterIndex = 0;
        }

        private void OnChapterSelectionChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && picker.SelectedItem is ChapterUiModel chapter && chapter.Id > 0 && _viewModel.ChapterIndex >= 0)
                collectionView.ScrollTo(0, chapter.Id - 1, position: ScrollToPosition.Start, animate: true);
        }

        private void OnVerseSelectionChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && picker.SelectedItem is VerseUiModel verse)
                collectionView.ScrollTo(verse.Id - 1, _viewModel.ChapterIndex, position: ScrollToPosition.Center, animate: true);
        }

        private void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            if (e.FirstVisibleItemIndex != 0)
                _viewModel.ChapterIndex = -1;
        }

        private void OnSwipeLeft(object sender, SwipedEventArgs e)
        {
            if (_viewModel.Bible?[_viewModel.BookIndex]?.Count > bibleChapterPicker.SelectedIndex + 1)
                bibleChapterPicker.SelectedIndex++;
        }

        private void OnSwipeRight(object sender, SwipedEventArgs e)
        {
            if (bibleChapterPicker.SelectedIndex > 0)
                bibleChapterPicker.SelectedIndex--;
        }
    }
}
