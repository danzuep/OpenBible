using Bible.Core.Abstractions;
using Bible.App.Models;

namespace Bible.App.Abstractions
{
    public interface IUiDataService : IDataService<BibleUiModel>
    {
        Task<BibleUiModel> LoadAsync(string fileName);
    }
}