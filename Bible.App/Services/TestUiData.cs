using Bible.Interfaces;
using BibleApp.Models;

namespace Bible.Reader.Services
{
    public sealed class TestUiData : IDataService<VerseUiModel>
    {
        public IReadOnlyList<VerseUiModel> LoadVerses(int chapterNumber)
        {
            int verseCount = Random.Shared.Next(2, 176);
            var verses = Enumerable.Range(1, verseCount).Select(v =>
                new VerseUiModel
                {
                    Id = v,
                    Text = $"Chapter #{chapterNumber}, Verse #{v}."
                });
            return verses.ToArray();
        }
    }
}
