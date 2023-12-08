using Bible.App.Abstractions;
using Bible.App.Pages;
using Bible.App.ViewModels;
using Bible.Reader.Services;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Bible.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "Material");
                    fonts.AddFont("MaterialIconsOutlined-Regular.ttf", "MaterialOutlined");
                });

            builder.Services.Register();

            return builder.Build();
        }

        public static T? GetService<T>() =>
            Ioc.Default.GetService<T>();

        public static T GetRequiredService<T>() where T : notnull =>
            Ioc.Default.GetRequiredService<T>();

        static IServiceProvider Register(this IServiceCollection services)
        {
#if DEBUG
            services.AddLogging(o => o.AddDebug());
#endif
            // Services
            services.AddSingleton<WebZefaniaService>();
            services.AddSingleton<IUiDataService, BibleUiDataService>();
            //services.AddSingleton<IDataService<BibleModel>, BibleReader>()

            // Views and ViewModels
            services.Register<MainPage, MainPageViewModel>();

            var provider = services.BuildServiceProvider();
            Ioc.Default.ConfigureServices(provider);

            return provider;
        }

        static void Register<TView>(this IServiceCollection services)
            where TView : class, IView
        {
            // View
            services.AddSingleton<TView>();
            // Route
            var type = typeof(TView);
            Routing.RegisterRoute(type.Name, type);
        }

        static void Register<TView, TViewModel>(this IServiceCollection services)
            where TView : class, IView
            where TViewModel : class, INotifyPropertyChanged
        {
            // ViewModel
            services.AddTransient<TViewModel>();
            services.Register<TView>();
        }
    }
}
