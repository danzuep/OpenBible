namespace BibleApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Register<MainPage>();
        }

        void Register<T>() where T : class
        {
            var type = typeof(T);
            Routing.RegisterRoute(type.Name, type);
        }
    }
}
