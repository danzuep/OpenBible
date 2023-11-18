using Bible.Core.Models;
using Bible.Data.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BibleApp
{
    public sealed partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<BibleBook> bibleBooks;

        [ObservableProperty]
        private BibleBook? selectedBook;

        [ObservableProperty]
        private BibleChapter? selectedChapter;

        public string Title { get; }

        public MainPageViewModel()
        {
            var bibleBooks = SerializerService.GetBibleBooks().ToArray();
            BibleBooks = new ObservableCollection<BibleBook>(bibleBooks);
            SelectedBook = bibleBooks.FirstOrDefault();
            Title = SelectedBook?.Reference.Translation ?? string.Empty;
        }

        //[ObservableProperty]
        //private ObservableCollection<VerseContent>? verseContents;

        //[RelayCommand] private void OnSelectionChanged() {}

        //private static IList<VerseContent> GetVerseCollection(BibleChapter? bibleChapter)
        //{
        //    var chapterContents = new List<VerseContent>();
        //    if (bibleChapter?.Verses != null)
        //    {
        //        foreach (var verse in bibleChapter.Verses)
        //        {
        //            chapterContents.Add(new VerseContent
        //            {
        //                VerseNumber = verse.Number,
        //                VerseText = verse.Text,
        //            });
        //        }
        //    }
        //    return chapterContents;
        //}

        //internal static ObservableCollection<VerseContent> GetVerseCollectionFromBibleChapter(BibleChapter? bibleChapter) =>
        //    new ObservableCollection<VerseContent>(GetVerseCollection(bibleChapter));
    }
}