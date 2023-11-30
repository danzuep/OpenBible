using BibleApp.Pages;

namespace BibleApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Register<MainPage>();
            Register<ChapterPage>();
        }

        void Register<T>() where T : class
        {
            var type = typeof(T);
            Routing.RegisterRoute(type.Name, type);
        }
    }
}
