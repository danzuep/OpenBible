using BibleApp.Models;

namespace Bible.Interfaces
{
    public interface IDataService<T>
    {
        IReadOnlyList<VerseUiModel> LoadVerses(int chapterNumber);
    }
}