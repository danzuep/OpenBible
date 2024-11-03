namespace Bible.App.Abstractions
{
    public interface IStorageService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value);
        bool Remove(string key);
        Task<Dictionary<string, string>> GetAsync(IEnumerable<string> keys);
        Task SetAsync(IReadOnlyDictionary<string, string> keyValuePairs);
        bool RemoveAll(IEnumerable<string>? keys = null);
    }
}