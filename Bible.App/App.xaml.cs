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
                    //ViewModels
                    .AddTransient<MainPageViewModel>()
                    .BuildServiceProvider());
            }

            MainPage = new AppShell();
        }
    }
}
