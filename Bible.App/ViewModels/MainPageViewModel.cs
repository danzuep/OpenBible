using Bible.Reader;
using BibleApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BibleApp.ViewModels
{
    public sealed partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private BibleUiModel? bibleModel;

        [ObservableProperty]
        private BookUiModel? selectedBook;

        [ObservableProperty]
        private ChapterUiModel? selectedChapter;

        //[ObservableProperty]
        //private ObservableCollection<VerseUiModel>? bibleVerses;

        private const string _testUsxBook = "zho/OCCB/GEN";

        [ObservableProperty]
        private ObservableCollection<string> translations = [
            "eng/WEB",
            "eng/WEBBE",
            "eng/WEBME",
            "chi/CUV",
            "chi/CUVS",
            //_testUsxBook,
            "tha/KJVTHAI"];

        [ObservableProperty]
        private string selectedTranslation;

        [ObservableProperty]
        private int bookIndex;

        [ObservableProperty]
        private int chapterIndex;

        private int _bookIndex => BookIndex < 0 ? 0 : BookIndex;
        private int _chapterIndex => ChapterIndex < 0 ? 0 : ChapterIndex;

        internal BibleUiModel _bible => BibleModel ?? new();
        internal BookUiModel _bibleBook => _bible.Books[_bookIndex];
        internal ChapterUiModel _bibleChapter => _bibleBook.Chapters[_chapterIndex];

        public MainPageViewModel()
        {
            SelectedTranslation = Translations[0];
        }

        internal void LoadSelectedBible(string translation)
        {
            //BibleReader.TransformUsx2Xml($"{_testUsxBook}.usx");
            var bible = translation != _testUsxBook ?
                BibleReader.LoadZefBible(translation) :
                BibleReader.LoadUsxBible(translation);
            BibleModel = new() { Translation = translation };
            foreach (var book in bible.Books)
            {
                var bibleBook = new BookUiModel { Id = book.Id, Name = book.Reference.BookName };
                foreach (var chapter in book.Chapters)
                {
                    var bibleChapter = new ChapterUiModel { Id = chapter.Id };
                    foreach (var verse in chapter.Verses)
                    {
                        var bibleVerse = new VerseUiModel { Id = verse.Id, Text = verse.Text };
                        bibleChapter.Verses.Add(bibleVerse);
                    }
                    bibleBook.Chapters.Add(bibleChapter);
                }
                BibleModel.Books.Add(bibleBook);
            }
        }
    }
}