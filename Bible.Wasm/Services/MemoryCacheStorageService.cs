using System.Text.Json;
using Bible.Backend.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Bible.Wasm.Services
{
    public class MemoryCacheStorageService : IStorageService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheStorageService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<string> GetItemAsync(string key)
        {
            var item = _memoryCache.Get(key) as string;
            return Task.FromResult(item ?? string.Empty);
        }

        public async Task<T?> GetSerializedItemAsync<T>(string key)
        {
            var json = await GetItemAsync(key);
            if (string.IsNullOrEmpty(json)) return default;
            return JsonSerializer.Deserialize<T>(json);
        }

        public Task SetItemAsync(string key, string value)
        {
            _memoryCache.Set(key, value);
            return Task.CompletedTask;
        }

        public async Task SetSerializedItemAsync<T>(string key, T value)
        {
            if (value == null) return;
            var json = JsonSerializer.Serialize(value);
            await SetItemAsync(key, json);
        }
    }
}
