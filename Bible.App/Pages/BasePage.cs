using System.ComponentModel;

namespace Bible.App.Pages
{
    public abstract class BasePage<TViewModel> : BasePage where TViewModel : class, INotifyPropertyChanged
    {
        public TViewModel ViewModel { get; private set; }

        protected BasePage(TViewModel? viewModel = null) : base()
        {
            ViewModel = viewModel ?? MauiProgram.GetRequiredService<TViewModel>();
            BindingContext = ViewModel;
        }
    }

    public abstract class BasePage : ContentPage
    {
        protected BasePage()
        {
            //if (string.IsNullOrWhiteSpace(Title))
            //    Title = GetType().Name;
        }
    }
}
