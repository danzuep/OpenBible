using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace Bible.App.ViewModels
{
    public abstract class BaseViewModel : ObservableObject
    {
        public ICommand NavigateCommand => new Command<Type>(
            async (type) => await AppShell.Current.GoToAsync(type.Name));
    }
}
