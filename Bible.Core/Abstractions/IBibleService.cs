using Bible.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bible.Interfaces
{
    public interface IBibleService
    {
        IReadOnlyList<BibleBook> BibleBooks { get; }
        BibleBook? GetBook(string bookName);
        BibleChapter? GetChapter(BibleBook bibleBook, int chapterNumber);
        BibleVerse? GetVerse(BibleChapter bibleBook, int verseNumber);
        ////TODO: what about alias book names and references?
        //BibleReference? GetReference(string bookChapterVerses);
        //IReadOnlyList<BibleVerse> GetVerses(BibleReference bibleReference);
    }
}
