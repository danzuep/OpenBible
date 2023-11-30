using BibleApp.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace BibleApp.Pages
{
    public sealed partial class MainPage : ContentPage
    {
        //public new MainPageViewModel BindingContext => _viewModel;

        //private readonly MainPageViewModel _viewModel;

        public MainPage()
        {
            InitializeComponent();
            //_viewModel = Ioc.Default.GetRequiredService<MainPageViewModel>();
            BindingContext = Ioc.Default.GetRequiredService<MainPageViewModel>();
        }
    }
}
