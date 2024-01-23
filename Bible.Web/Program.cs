using Bible.Core.Abstractions;
using Bible.Core.Models;
using Bible.Reader.Services;
using Bible.Web.Abstractions;
using Bible.Web.Components;
using Bible.Web.Services;

namespace Bible.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            RegisterIoC(builder.Services);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.Run();
        }

        static IServiceCollection RegisterIoC(IServiceCollection services)
        {
            services.AddHttpClient();
#if DEBUG
            services.AddLogging(o => o.AddDebug());
#endif
            services.AddSingleton<WebZefaniaService>();
            services.AddSingleton<IUiDataService, BibleUiDataService>();
            services.AddSingleton<IDataService<BibleModel>, BibleReader>();

            return services;
        }
    }
}
