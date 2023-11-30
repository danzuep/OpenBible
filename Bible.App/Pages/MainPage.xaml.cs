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
            Verses = LoadVerses(1);
        }

        public IReadOnlyList<string> Verses { get; private set; }

        private IReadOnlyList<string> LoadVerses(int chapterNumber)
        {
            int verseCount = Random.Shared.Next(2, 176);
            var verses = Enumerable.Range(1, verseCount)
                .Select(v => $"Chapter #{chapterNumber}, Verse #{v}.");
            return verses.ToArray();
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && picker.SelectedItem is int chapter)
            {
                Verses = LoadVerses(chapter);
                OnPropertyChanged(nameof(Verses));
            }
        }
    }
}
