using Bible.Backend.Services;
using Microsoft.JSInterop;

namespace Bible.Wasm.Services
{
    public class JsStorageService : StorageService
    {
        private static readonly string _jsStorage = "window.cookieStorage";
        private static readonly string _jsGetItem = $"{_jsStorage}.get";
        private static readonly string _jsSetItem = $"{_jsStorage}.set";

        private readonly IJSRuntime _jsRuntime;

        public JsStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async override Task<string> GetItemAsync(string key)
        {
            return await _jsRuntime.InvokeAsync<string>(_jsGetItem, key);
        }

        public async override Task SetItemAsync(string key, string value)
        {
            await _jsRuntime.InvokeVoidAsync(_jsSetItem, key, value);
        }
    }
}
