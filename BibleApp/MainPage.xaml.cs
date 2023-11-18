using Bible.Core.Models;

namespace BibleApp
{
    public partial class MainPage : ContentPage
    {
        readonly MainPageViewModel _viewModel = new();
        bool _selectionEnabled;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
            Title = _viewModel.Title;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private void OnBookSelectionChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && picker.SelectedItem is BibleBook selectedBook)
            {
                _viewModel.SelectedChapter = selectedBook.Chapters.FirstOrDefault();
            }
        }

        //private void OnChapterSelectionChanged(object sender, EventArgs e)
        //{
        //    if (sender is Picker picker && picker.SelectedItem is BibleChapter selectedChapter)
        //    {
        //        _viewModel.VerseContents = MainPageViewModel.GetVerseCollectionFromBibleChapter(selectedChapter);
        //    }
        //}
    }
}
