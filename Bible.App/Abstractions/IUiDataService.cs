using Bible.App.Models;

namespace Bible.App.Abstractions
{
    public interface IUiDataService
    {
        Task<BibleUiModel> LoadFileAsync(string fileName);
    }
}