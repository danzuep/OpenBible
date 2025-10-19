using Bible.Backend;
using Bible.Backend.Abstractions;
using Bible.Backend.Services;
using Bible.Razor.Services;
using Bible.Usx.Services;
using Bible.Wasm.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace Bible.Wasm
{
    public sealed class Program : ServiceProviderBase
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            RegisterServices(builder.Services);
            //await ResourceHelper.SplitUnihanReadingsToFilesAsync();

            await builder.Build().RunAsync();
        }

        static void RegisterServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddMudServices();
            services.AddScoped<JsScrollService>();
            services.AddScoped<IDimensionService, JsDimensionService>();
            services.AddScoped<IDownloadService, JsDownloadService>();
            services.AddSingleton<IBibleDataService, DataService>();
            services.AddSingleton<BibleBookService>();

            //services.AddSingleton<IStorageService, JsStorageService>();
            //services.AddSingleton<IStorageService, DictionaryStorageService>();
            services.AddSingleton<IStorageService, MemoryCacheStorageService>();

            RegisterIoC(services);
        }
    }
}
