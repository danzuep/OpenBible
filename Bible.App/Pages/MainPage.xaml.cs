using BibleApp.Models;

namespace BibleApp.Pages
{
    public sealed partial class MainPage : ContentPage
    {
        public MainPage() : base()
        {
            InitializeComponent();
            BindingContext = this;
            if (string.IsNullOrWhiteSpace(Title))
                Title = GetType().Name;
            chapterPicker.ItemsSource = Enumerable.Range(1, 150).ToArray();
            chapterPicker.SelectedIndex = 0;
        }

        public Chapter? Chapter { get; private set; }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && picker.SelectedItem is int chapterNumber)
            {
                Chapter = Data.LoadChapter(chapterNumber);
                OnPropertyChanged(nameof(Chapter));
            }
        }

        private async void OnClickedEvent(object sender, EventArgs e)
        {
            await Task.CompletedTask;
            var parameters = new Dictionary<string, object?> { { "Chapter", Chapter } };
            await Shell.Current.GoToAsync(nameof(ChapterPage), parameters);
        }

        private void OnClickedEventScroll(object sender, EventArgs e)
        {
            collectionView.ScrollTo(12, position: ScrollToPosition.Start);
        }
    }
}
