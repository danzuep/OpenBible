using System.Collections.Concurrent;
using System.Text.Json;
using Bible.Backend.Abstractions;

namespace Bible.Backend.Services
{
    public class DictionaryStorageService : IStorageService
    {
        private readonly ConcurrentDictionary<string, string> _dictionary = new();

        public Task<string> GetItemAsync(string key)
        {
            _dictionary.TryGetValue(key, out var item);
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
            _dictionary.AddOrUpdate(
                key,
                value,
                (existingKey, existingValue) => value
            );
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
