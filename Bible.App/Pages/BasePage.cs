using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace BibleApp.Pages
{
    public abstract class BasePage<TViewModel> : BasePage where TViewModel : ObservableObject
    {
        public TViewModel ViewModel { get; }

        protected BasePage(TViewModel? viewModel = null) : base()
        {
            ViewModel = viewModel ?? Ioc.Default.GetRequiredService<TViewModel>();
            BindingContext = ViewModel;
        }
    }

    public abstract class BasePage : ContentPage
    {
        protected BasePage()
        {
            if (string.IsNullOrWhiteSpace(Title))
                Title = GetType().Name;
        }
    }
}
