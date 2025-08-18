using System.Text.Json;
using Bible.Backend.Abstractions;
using Microsoft.JSInterop;

namespace Bible.Wasm.Services
{

    public class JsStorageService : IStorageService
    {
        private static readonly string _jsGetItem = "localStorage.getItem";
        private static readonly string _jsSetItem = "localStorage.setItem";

        private readonly IJSRuntime _jsRuntime;

        public JsStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string> GetItemAsync(string key)
        {
            return await _jsRuntime.InvokeAsync<string>(_jsGetItem, key);
        }

        public async Task<T?> GetSerializedItemAsync<T>(string key)
        {
            var json = await GetItemAsync(key);
            if (string.IsNullOrEmpty(json)) return default;
            return JsonSerializer.Deserialize<T>(json);
        }

        public async Task SetItemAsync(string key, string value)
        {
            await _jsRuntime.InvokeVoidAsync(_jsSetItem, key, value);
        }

        public async Task SetSerializedItemAsync<T>(string key, T value)
        {
            if (value == null) return;
            var json = JsonSerializer.Serialize(value);
            await SetItemAsync(key, json);
        }
    }
}
