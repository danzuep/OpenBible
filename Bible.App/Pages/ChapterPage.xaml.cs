using BibleApp.Models;

namespace BibleApp.Pages
{
    [QueryProperty(nameof(Models.Chapter), "Chapter")]
    public partial class ChapterPage : ContentPage
    {
        public ChapterPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        Chapter? _chapter;
        public Chapter? Chapter
        {
            get => _chapter ?? Data.LoadChapter(1);
            set
            {
                _chapter = value;
                OnPropertyChanged();
            }
        }
    }
}
