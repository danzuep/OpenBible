using Bible.Interfaces;
using Bible.Reader.Services;
using BibleApp.Models;
using BibleApp.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace BibleApp
{
    public partial class App : Application
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
#if DEBUG
                    //.AddSingleton<IDataService<BibleUiModel>, TestUiData>()
                    .AddSingleton<IDataService<BibleUiModel>, BibleUiData>()
#else
                    //.AddSingleton<IBibleService, BibleReader>()
                    .AddSingleton<IDataService<BibleUiModel>, BibleUiData>()
#endif
                    //ViewModels
                    .AddTransient<MainPageViewModel>()
                    .BuildServiceProvider());
            }

            MainPage = new AppShell();
        }
    }
}
