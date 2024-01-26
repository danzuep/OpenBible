using Bible.Core.Models;
using Bible.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Bible.Web
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

        static IServiceCollection RegisterIoC(IServiceCollection services)
        {
#if DEBUG
            services.AddLogging(o => o.AddDebug());
#endif
            // Services
            services.AddSingleton<IDataService<IEnumerable<BibleBook>>, DataService>();

            return services;
        }
    }
}
