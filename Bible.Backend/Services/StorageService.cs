using System.Text.Json;
using Bible.Backend.Abstractions;

namespace Bible.Backend.Services
{
    public abstract class StorageService : IStorageService
    {
        public abstract Task<string> GetItemAsync(string key);

        public abstract Task SetItemAsync(string key, string value);

        public virtual async Task<T?> GetSerializedItemAsync<T>(string key)
        {
            var json = await GetItemAsync(key);
            if (string.IsNullOrEmpty(json)) return default;
            return JsonSerializer.Deserialize<T>(json);
        }

        public virtual async Task SetSerializedItemAsync<T>(string key, T value)
        {
            if (value == null) return;
            var json = JsonSerializer.Serialize(value);
            await SetItemAsync(key, json);
        }
    }
}
