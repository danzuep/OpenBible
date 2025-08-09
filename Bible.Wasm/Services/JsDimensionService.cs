using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Bible.Wasm.Services
{
    public interface IDimensionService
    {
        Task<RectangleSize> StartAsync(Action<RectangleSize> action);
    }

    /// <inheritdoc cref="System.Drawing.Size"/>
    public record struct RectangleSize(int Width, int Height);

    public class JsDimensionOptions
    {
        public int ReportRate { get; set; } = 300;
    }

    /// <seealso href="https://github.com/EdCharbeneau/BlazorSize/blob/master/BlazorSize/Resize/ResizeListener.cs"/>
    public class JsDimensionService : IDimensionService
    {
        private Action<RectangleSize>? _action;
        private static readonly string _jsGetMethod = "window.dimensions.get";
        private static readonly string _jsEventMethod = "window.dimensions.eventListener";
        private readonly Lazy<ValueTask<RectangleSize>> _jsGetFunction;
        private readonly Lazy<ValueTask> _jsEventFunction;
        private readonly IJSRuntime _jsInterop;
        private readonly JsDimensionOptions _options;

        public JsDimensionService(IJSRuntime jsRuntime, IOptions<JsDimensionOptions>? options = null)
        {
            _jsInterop = jsRuntime;
            _options = options?.Value ?? new JsDimensionOptions();
            // Create a reference to the current object.
            var reference = DotNetObjectReference.Create(this);
            // Create a task that will call the JS "dimensions.get" function when run.
            _jsGetFunction = new(() => _jsInterop.InvokeAsync<RectangleSize>(_jsGetMethod, reference));
            // Create a task that will call the JS "dimensions.eventListener" function when run.
            // That function looks for the [JSInvokable] dotnet method "OnWindowDimensionsUpdated".
            _jsEventFunction = new(() => _jsInterop.InvokeVoidAsync(_jsEventMethod, reference, _options.ReportRate));
        }

        /// <summary>
        /// Subscribe to the "resize" event. This will do work when the browser is resized.
        /// Returns the initial RectangleSize, this includes the Height and Width of the document.
        /// </summary>
        public async Task<RectangleSize> StartAsync(Action<RectangleSize> action)
        {
            var size = await _jsGetFunction.Value.ConfigureAwait(false);
            if (action != null)
                _action = action;
            await _jsEventFunction.Value;
            return size;
        }

        // This method will be called when the window resizes.
        // It is ONLY called when the user stops dragging the window's edge. (It is already throttled to protect your app from performance nightmares)
        [JSInvokable]
        public Task OnWindowDimensionsUpdated(RectangleSize size)
        {
            // Execute the action, e.g. store the browsers's width and height and re-render.
            _action?.Invoke(size);
            // Handle the updated window dimensions here
            // You can access the dimensions using the 'dimensions' object passed from JavaScript
            return Task.CompletedTask;
        }
    }
}
