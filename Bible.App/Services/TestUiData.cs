using Bible.Interfaces;
using BibleApp.Models;
using System.Collections.ObjectModel;

namespace Bible.Reader.Services
{
    public sealed class TestUiData : IDataService<BibleUiModel>
    {
        public BibleUiModel Load(string fileName, string suffix = ".xml") =>
            LoadMock(66, suffix.Length * 3, fileName.Length * 3);

        public BibleUiModel LoadMock(int bookCount, int chapterCount, int verseCount, string name = "Books, Chapters, and Verses")
        {
            var books = Enumerable.Range(1, bookCount); //ParallelEnumerable
            var chapters = Enumerable.Range(1, chapterCount);
            var verses = Enumerable.Range(1, verseCount);
            var bible = new BibleUiModel
            {
                Translation = name,
                Books = new ObservableCollection<BookUiModel>(books.Select(b =>
                    new BookUiModel
                    {
                        Id = b,
                        Name = $"Book #{b}",
                        ChapterCount = chapterCount,
                        Chapters = new ObservableCollection<ChapterUiModel>(chapters.Select(c =>
                            new ChapterUiModel
                            {
                                Id = c,
                                Verses = new ObservableCollection<VerseUiModel>(verses.Select(v =>
                                    new VerseUiModel
                                    {
                                        Id = v,
                                        Text = $"Book #{b}, Chapter #{c}, Verse #{v}."
                                    }).ToArray())
                            }).ToArray())
                    }).ToArray())
            };
            return bible;
        }
    }
}
