using Bible.App.Models;
using Bible.App.ViewModels;
using Bible.App.Views;

namespace Bible.App.Pages
{
    public sealed partial class MainPage : BasePage<MainPageViewModel>
    {
        public MainPage() : base()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.TranslationIndex = 0;
            bibleChapterPicker.Focus();
        }

        private void OnBookSelectionChanged(object sender, EventArgs e)
        {
            ViewModel.ChapterIndex = 0;
        }

        bool isChapterViewChange;

        private void OnChapterSelectionChanged(object sender, EventArgs e)
        {
            if (isChapterViewChange)
                isChapterViewChange = false;
            else if (sender is Picker picker && picker.SelectedItem is ChapterUiModel chapter && chapter.Id > 0)
                chapterCollectionView.ScrollTo(chapter.Id - 1, position: ScrollToPosition.Start, animate: false);
        }

        private void OnChapterCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            if (ViewModel.ChapterIndex != e.FirstVisibleItemIndex)
            {
                isChapterViewChange = true;
                ViewModel.ChapterIndex = e.FirstVisibleItemIndex;
            }
        }

        private async void OnChapterClickedEvent(object sender, EventArgs e)
        {
            await Task.CompletedTask;
            if (sender is Picker picker && picker.SelectedItem is ChapterUiModel chapter)
            {
                var parameters = new Dictionary<string, object?> { { "Chapter", chapter } };
                await Shell.Current.GoToAsync(nameof(ChapterView), parameters);
            }
        }

        private void OnSwipeLeft(object sender, SwipedEventArgs e)
        {
            if (ViewModel.BookIndex >= 0 && ViewModel.Bible?[ViewModel.BookIndex]?.Count > ViewModel.ChapterIndex + 1)
                ViewModel.ChapterIndex++;
        }

        private void OnSwipeRight(object sender, SwipedEventArgs e)
        {
            if (ViewModel.ChapterIndex > 0)
                ViewModel.ChapterIndex--;
        }
    }
}
