using Bible.App.Models;
using Bible.Reader.Models;

namespace Bible.App.Abstractions
{
    public interface IUiDataService
    {
        Task<BibleUiModel> LoadFileAsync(string fileName);
        Task<BibleUiModel?> GetBibleAsync(string identifier, CancellationToken cancellationToken = default);
        IAsyncEnumerable<WebBibleInfoModel> AsyncGetBibleInfo(CancellationToken cancellationToken = default);
    }
}