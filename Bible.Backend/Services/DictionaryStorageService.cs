using System.Collections.Concurrent;

namespace Bible.Backend.Services
{
    public class DictionaryStorageService : StorageService
    {
        private readonly ConcurrentDictionary<string, string> _dictionary = new();

        public override Task<string> GetItemAsync(string key)
        {
            _dictionary.TryGetValue(key, out var item);
            return Task.FromResult(item ?? string.Empty);
        }

        public override Task SetItemAsync(string key, string value)
        {
            _dictionary.AddOrUpdate(
                key,
                value,
                (existingKey, existingValue) => value
            );
            return Task.CompletedTask;
        }
    }
}
