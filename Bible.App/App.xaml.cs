using Bible.App.Abstractions;
using Bible.App.ViewModels;
using Bible.Core.Abstractions;
using Bible.Core.Models;
using Bible.Reader.Services;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace BibleApp
{
    public sealed partial class App : Application
    {
        private bool _initialized;

        public App()
        {
            InitializeComponent();

            // Register services
            if (!_initialized)
            {
                _initialized = true;
                Ioc.Default.ConfigureServices(
                    new ServiceCollection()
                    //Services
                    //.AddSingleton<IDataService<BibleModel>, BibleReader>()
                    .AddSingleton<IUiDataService, BibleUiData>()
#if DEBUG
                    //.AddSingleton<IUiDataService, TestUiData>()
#endif
                    //ViewModels
                    .AddTransient<MainPageViewModel>()
                    .BuildServiceProvider());
            }

            MainPage = new AppShell();
        }
    }
}
