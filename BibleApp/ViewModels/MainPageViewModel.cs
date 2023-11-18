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

        [ObservableProperty]
        private string title;

        public MainPageViewModel()
        {
            var bibleBooks = SerializerService.GetBibleBooks().ToArray();
            BibleBooks = new ObservableCollection<BibleBook>(bibleBooks);
            SelectedBook = bibleBooks.FirstOrDefault();
            Title = SelectedBook?.Reference.Translation ?? string.Empty;
        }
    }
}