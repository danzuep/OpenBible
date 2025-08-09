using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Bible.Wasm.Services
{
    public class JsScrollOptions
    {
        public int ThresholdPixels { get; set; } = 100;
    }

    public class JsScrollService
    {
        private static readonly string _jsGetScrollTop = "window.scroll.getTop";
        private static readonly string _jsSetScrollTop = "window.scroll.setTop";
        private static readonly string _jsGetScrollHeight = "window.scroll.getHeight";
        private static readonly string _jsGetClientHeight = "window.scroll.getClientHeight";

        private readonly IJSRuntime _jsRuntime;
        private readonly JsScrollOptions _options;

        public JsScrollService(IJSRuntime jsRuntime, IOptions<JsScrollOptions>? options = null)
        {
            _jsRuntime = jsRuntime;
            _options = options?.Value ?? new JsScrollOptions();
        }

        public async Task<int> GetScrollTopAsync(ElementReference scrollContainer)
        {
            return await _jsRuntime.InvokeAsync<int>(_jsGetScrollTop, scrollContainer);
        }

        public async Task SetScrollTopAsync(ElementReference scrollContainer, int newScrollTop)
        {
            await _jsRuntime.InvokeVoidAsync(_jsSetScrollTop, scrollContainer, newScrollTop);
        }

        public async Task<int> GetScrollHeightAsync(ElementReference scrollContainer)
        {
            return await _jsRuntime.InvokeAsync<int>(_jsGetScrollHeight, scrollContainer);
        }

        public async Task<int> GetClientHeightAsync(ElementReference scrollContainer)
        {
            return await _jsRuntime.InvokeAsync<int>(_jsGetClientHeight, scrollContainer);
        }

        public async Task<bool> IsNearBottomAsync(ElementReference scrollContainer)
        {
            var scrollHeight = await GetScrollHeightAsync(scrollContainer);
            var scrollTop = await GetScrollTopAsync(scrollContainer);
            var clientHeight = await GetClientHeightAsync(scrollContainer);

            return (scrollHeight - scrollTop - clientHeight) < _options.ThresholdPixels;
        }
    }
}
