using BibleApp.Models;

namespace BibleApp
{
    internal class Data
    {
        internal static IList<string> LoadVerses(int chapterNumber)
        {
            int verseCount = Random.Shared.Next(2, 176);
            var verses = Enumerable.Range(1, verseCount)
                .Select(v => $"Chapter #{chapterNumber}, Verse #{v}.");
            return verses.ToArray();
        }

        internal static Chapter LoadChapter(int chapterNumber)
        {
            var chapter = new Chapter
            {
                Id = chapterNumber,
                Verses = LoadVerses(chapterNumber)
            };
            return chapter;
        }
    }
}
