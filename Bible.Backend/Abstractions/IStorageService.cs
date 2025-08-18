namespace Bible.Backend.Abstractions
{
    public interface IStorageService
    {
        Task<string> GetItemAsync(string key);
        Task<T?> GetSerializedItemAsync<T>(string key);
        Task SetItemAsync(string key, string value);
        Task SetSerializedItemAsync<T>(string key, T value);
    }
}
