using System.Text.Json;
using Bible.App.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bible.App.Services;

public sealed class StorageService : IStorageService
{
    private readonly ISecureStorage _secureStorage;
    private readonly ILogger<StorageService> _logger;

    public StorageService(ISecureStorage secureStorage, ILogger<StorageService>? logger = null)
    {
        _secureStorage = secureStorage;
        _logger = logger ?? NullLogger<StorageService>.Instance;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return default;
        }
        var value = await _secureStorage.GetAsync(key);
        if (value == null)
        {
            return default;
        }
        try
        {
            return JsonSerializer.Deserialize<T>(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize value for '{0}'", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value)
    {
        if (!string.IsNullOrWhiteSpace(key) && value != null)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                await _secureStorage.SetAsync(key, json);
            }
            catch (NotSupportedException ex)
            {
                _logger.LogError(ex, "Failed to serialize value for '{0}'", key);
            }
        }
    }

    public async Task<Dictionary<string, string>> GetAsync(IEnumerable<string> keys)
    {
        var results = new Dictionary<string, string>();
        if (keys != null)
        {
            foreach (var key in keys)
            {
                var value = await _secureStorage.GetAsync(key);
                results.Add(key, value ?? string.Empty);
            }
        }
        return results;
    }

    public async Task SetAsync(IReadOnlyDictionary<string, string> keyValuePairs)
    {
        if (keyValuePairs != null)
        {
            foreach (var pair in keyValuePairs)
            {
                await _secureStorage.SetAsync(pair.Key, pair.Value ?? string.Empty);
            }
        }
    }

    public bool Remove(string key)
    {
        bool isRemoved = false;
        if (!string.IsNullOrWhiteSpace(key))
        {
            isRemoved = _secureStorage.Remove(key);
        }
        return isRemoved;
    }

    public bool RemoveAll(IEnumerable<string>? keys = null)
    {
        int count = 0;
        bool isRemoved = false;
        if (keys == null)
        {
            _secureStorage.RemoveAll();
        }
        else
        {
            foreach (var key in keys)
            {
                if (_secureStorage.Remove(key) && !isRemoved)
                {
                    isRemoved = true;
                }
                count++;
            }
        }
        return isRemoved || count == 0;
    }
}
