using BibleApp.Models;

namespace BibleApp.Views
{
    [QueryProperty(nameof(BookUiModel), "Book")]
    public sealed partial class BookView : ContentView
    {
        public BookView()
        {
            InitializeComponent();
            BindingContext = this;
        }

        BookUiModel? _book;
        public BookUiModel? Book
        {
            get => _book;
            set
            {
                _book = value;
                OnPropertyChanged();
            }
        }
        
        private void OnChapterSelectionChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && picker.SelectedItem is ChapterUiModel chapter && chapter.Id > 0)
                chapterCollectionView.ScrollTo(chapter.Id - 1, position: ScrollToPosition.Start, animate: false);
        }

        private void OnChapterCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            //if (e.FirstVisibleItemIndex != 0)
            //    ViewModel.ChapterIndex = -1;
        }
    }
}
