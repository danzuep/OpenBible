using Bible.Core.Models;
using Bible.Reader;
using BibleApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BibleApp.ViewModels
{
    public sealed partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private BibleUiModel? bible;

        [ObservableProperty]
        private BookUiModel? selectedBook;

        [ObservableProperty]
        private ChapterUiModel? selectedChapter;

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

        private byte _bookIndex => BookIndex < byte.MinValue ? byte.MinValue : (byte)BookIndex;
        private byte _chapterIndex => ChapterIndex < byte.MinValue ? byte.MinValue : (byte)ChapterIndex;

        private BibleModel? _bible;

        public MainPageViewModel()
        {
            SelectedTranslation = Translations[0];
        }

        private BibleModel LoadTranslation(string translation)
        {
            Bible = new() { Translation = translation };
            //BibleReader.TransformUsx2Xml($"{_testUsxBook}.usx");
            var bible = translation != _testUsxBook ?
                BibleReader.LoadZefBible(translation) :
                BibleReader.LoadUsxBible(translation);
            return bible;
        }

        internal void LoadBibleBooks()
        {
            _bible = LoadTranslation(SelectedTranslation);
            foreach (var book in _bible.Books)
            {
                var bibleBook = new BookUiModel
                {
                    Id = book.Id,
                    Name = book.Reference.BookName,
                    ChapterCount = book.ChapterCount
                };
                Bible?.Books.Add(bibleBook);
            }
        }

        private ChapterUiModel LoadChapter(byte bookIndex, byte chapterIndex)
        {
            var bibleChapter = new ChapterUiModel { Id = chapterIndex + 1 };
            if (_bible != null)
            {
                foreach (var verse in _bible.Books[bookIndex].Chapters[chapterIndex].Verses)
                {
                    var bibleVerse = new VerseUiModel { Id = verse.Id, Text = verse.Text };
                    bibleChapter.Verses.Add(bibleVerse);
                }
            }
            return bibleChapter;
        }

        internal ChapterUiModel LoadChapter() => LoadChapter(_bookIndex, _chapterIndex);
    }
}