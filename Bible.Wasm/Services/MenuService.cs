using Bible.Core.Models;
using Bible.Data.Models;
using Bible.Data.Services;
using Bible.Wasm.Models;

namespace Bible.Wasm.Services
{
    public interface IMenuService
    {
        Task<IReadOnlyList<BibleBookNav>> LoadAsync(string language = "eng");
    }

    public class MenuService : IMenuService
    {
        private IReadOnlyList<BibleBookNav>? _books;

        public async Task<IReadOnlyList<BibleBookNav>> LoadAsync(string language = "eng")
        {
            if (_books == null)
            {
                var resourceName = "Bible.Data.Json.BibleContentsCanonBookChapters.json";
                _books = await SerializerService.GetFromJsonResourceAsync<BibleBookNav[]>(resourceName) ?? [];
            }
            return _books;
        }
    }
}