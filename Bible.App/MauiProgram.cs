using System.ComponentModel;
using Bible.App.Abstractions;
using Bible.App.Models.Options;
using Bible.App.Pages;
using Bible.App.Services;
using Bible.App.ViewModels;
using Bible.Reader.Services;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

            var configuration = BuildConfiguration();
            builder.Configuration.AddConfiguration(configuration);

            builder.Services.ConfigureLogging();
            builder.Services.RegisterServices();

            return builder.Build();
        }

        public static T? GetService<T>() =>
            Ioc.Default.GetService<T>();

        public static T GetRequiredService<T>() where T : notnull =>
            Ioc.Default.GetRequiredService<T>();

        static IServiceProvider RegisterServices(this IServiceCollection services)
        {
            // Services
            services.AddSingleton((_) => SecureStorage.Default);
            services.AddSingleton<IStorageService, StorageService>();
            services.AddSingleton<WebZefaniaService>();
            services.AddSingleton<IUiDataService, BibleUiDataService>();
            //services.AddSingleton<IDataService<BibleModel>, BibleReader>()

            // Views and ViewModels
            services.Register<MainPage, MainPageViewModel>();

            var provider = services.BuildServiceProvider();
            Ioc.Default.ConfigureServices(provider);

            return provider;
        }

        public static IConfiguration BuildConfiguration(string? prefix = "Azure")
        {
            var configurationBuilder = new ConfigurationBuilder();
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
#if WINDOWS
                // Add configuration sources
                configurationBuilder.AddUserSecrets<App>();
                configurationBuilder.AddEnvironmentVariables(prefix);
#endif
            }
            var configuration = configurationBuilder.Build();
            return configuration;
        }

        static void ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
        {
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
#if WINDOWS
                // Bind typed configuration sources
                services.Configure<AppOptions>(configuration); //configuration.GetSection(AppOptions.SectionName)
#endif
            }
        }

        static void ConfigureLogging(this IServiceCollection services)
        {
            services.AddLogging(o =>
            {
                var appName = typeof(MauiProgram).Assembly.GetName().ToString();
#if ANDROID
                o.AddProvider(new AndroidLoggerProvider());
#else
                o.AddConsole();
#endif
#if DEBUG
                o.AddDebug().SetMinimumLevel(LogLevel.Debug);
#endif
            });
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
