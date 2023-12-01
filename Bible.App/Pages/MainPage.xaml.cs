using BibleApp.Models;
using BibleApp.ViewModels;
using BibleApp.Views;

namespace BibleApp.Pages
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

        private void OnChapterSelectionChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && picker.SelectedItem is ChapterUiModel chapter && chapter.Id > 0)
                chapterCollectionView.ScrollTo(chapter.Id - 1, position: ScrollToPosition.Start, animate: false);
        }

        private void OnChapterCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            if (ViewModel.ChapterIndex != e.FirstVisibleItemIndex)
                ViewModel.ChapterIndex = e.FirstVisibleItemIndex;
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
            if (ViewModel.Bible?[ViewModel.BookIndex]?.Count > bibleChapterPicker.SelectedIndex + 1)
                bibleChapterPicker.SelectedIndex++;
        }

        private void OnSwipeRight(object sender, SwipedEventArgs e)
        {
            if (bibleChapterPicker.SelectedIndex > 0)
                bibleChapterPicker.SelectedIndex--;
        }
    }
}
