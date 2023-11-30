using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace BibleApp.Pages
{
    public abstract class BasePage<TViewModel> : ContentPage where TViewModel : ObservableObject
    {
        public new TViewModel BindingContext => _viewModel;

        private readonly TViewModel _viewModel;

        protected BasePage(TViewModel? viewModel = null)
        {
            viewModel ??= Ioc.Default.GetRequiredService<TViewModel>();
            _viewModel = viewModel;
            if (string.IsNullOrWhiteSpace(Title))
                Title = GetType().Name;
        }
    }
}
