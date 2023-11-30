using BibleApp.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace BibleApp
{
    public sealed partial class MainPage : ContentPage
    {
        private MainPageViewModel _viewModel => (MainPageViewModel)BindingContext;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = Ioc.Default.GetRequiredService<MainPageViewModel>();
            _viewModel.TranslationIndex = 0;
            bibleChapterPicker.Focus();
        }

        private void OnSwipeLeft(object sender, SwipedEventArgs e)
        {
            if (_viewModel.SelectedBook?.Count > bibleChapterPicker.SelectedIndex + 1)
                bibleChapterPicker.SelectedIndex++;
        }

        private void OnSwipeRight(object sender, SwipedEventArgs e)
        {
            if (bibleChapterPicker.SelectedIndex > 0)
                bibleChapterPicker.SelectedIndex--;
        }
    }
}
