using Bible.Core.Models;
using Bible.Reader.Models;
using Bible.Reader;

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

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private void OnTranslationSelectionChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && picker.SelectedItem is string selectedTranslation)
            {
                _viewModel.BibleBooks = BibleReader.LoadBible<XmlZefania>(selectedTranslation).Books;
                _viewModel.SelectedBook = _viewModel.BibleBooks.FirstOrDefault();
                _viewModel.SelectedChapter = _viewModel.SelectedBook?.Chapters.FirstOrDefault();
            }
        }

        private void OnBookSelectionChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && picker.SelectedItem is BibleBook selectedBook)
            {
                _viewModel.SelectedChapter = selectedBook.Chapters.FirstOrDefault();
            }
        }

        private void OnSwipeLeft(object sender, SwipedEventArgs e)
        {
            if (_viewModel.SelectedChapter?.ChapterNumber is int chapter && _viewModel.SelectedBook is BibleBook book && book.ChapterCount > chapter)
            {
                _viewModel.SelectedChapter = _viewModel.SelectedBook.Chapters[chapter];
            }
        }

        private void OnSwipeRight(object sender, SwipedEventArgs e)
        {
            if (_viewModel.SelectedChapter?.ChapterNumber is int chapter && chapter > 1 && _viewModel.SelectedBook is not null)
            {
                _viewModel.SelectedChapter = _viewModel.SelectedBook.Chapters[chapter - 2];
            }
        }
    }
}
