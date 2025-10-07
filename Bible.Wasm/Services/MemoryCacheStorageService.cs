using Bible.Backend.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Bible.Wasm.Services
{
    public class MemoryCacheStorageService : StorageService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheStorageService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public override Task<string> GetItemAsync(string key)
        {
            var item = _memoryCache.Get(key) as string;
            return Task.FromResult(item ?? string.Empty);
        }

        public override Task SetItemAsync(string key, string value)
        {
            _memoryCache.Set(key, value);
            return Task.CompletedTask;
        }
    }
}
