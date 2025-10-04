using Bible.Backend.Abstractions;
using Bible.Backend.Services;
using Bible.Core.Abstractions;
using Bible.Core.Models;
using Bible.Usx.Services;
using Bible.Wasm.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace Bible.Wasm
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            RegisterIoC(builder.Services);

            await builder.Build().RunAsync();
        }

        static void RegisterIoC(IServiceCollection services)
        {
#if DEBUG
            services.AddLogging(o => o.AddDebug());
#endif
            // Services
            services.AddMudServices();
            services.AddScoped<IBibleBookNavService, BibleBookNavService>();
            services.AddScoped<IDeserializer, XDocDeserializer>();
            services.AddScoped<IBulkParser, UsxVersionParser>();
            services.AddScoped<IParser<BibleBook>, UsxToBibleBookParser>();
            services.AddScoped<IDataService<BibleModel>, UsxToBibleModelParser>();
            //services.AddScoped<IUnihanReadings, UnihanSerializer>();
            //services.AddScoped<IUsxVisitor, UsxToBibleBookVisitor>();
            services.AddScoped<JsScrollService>();
            services.AddScoped<IDimensionService, JsDimensionService>();
            services.AddScoped<IDownloadService, JsDownloadService>();
            services.AddSingleton<StreamDataService>();
            services.AddSingleton<IBibleDataService, DataService>();
            services.AddScoped<BasicDataService>();
            services.AddSingleton<BibleBookService>();
            //services.AddSingleton<UsjRenderVisitor>();
            services.AddSingleton<UsxToUsjConverter>();
            services.AddSingleton<IStorageService, JsStorageService>();
            services.AddSingleton<UsjBookService>();
            services.AddSingleton<UnihanService>();

            _provider = services.BuildServiceProvider();
        }

        static IServiceProvider? _provider;

        /// <inheritdoc cref="IServiceProvider.GetService(Type)"/>
        public static T? GetService<T>()
        {
            if (_provider == null)
                return default;
            return _provider.GetService<T>();
        }

        /// <inheritdoc cref="ServiceProviderServiceExtensions.GetRequiredService{T}(IServiceProvider)(Type)"/>
        public static T GetRequiredService<T>() where T : notnull
        {
            if (_provider == null)
                throw new ArgumentNullException(nameof(_provider));
            return _provider.GetRequiredService<T>() ??
                throw new InvalidOperationException($"There is no service of type {typeof(T).FullName}");
        }
    }
}
