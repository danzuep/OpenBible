using Bible.App.Models;

namespace Bible.App.Views
{
    [QueryProperty(nameof(ChapterUiModel), "Chapter")]
    public sealed partial class ChapterView : ContentView
    {
        public ChapterView()
        {
            InitializeComponent();
            BindingContext = Chapter;
        }

        public ChapterUiModel? Chapter { get; set; }
    }
}
