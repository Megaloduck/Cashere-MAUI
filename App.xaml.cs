namespace Cashere
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Initialize theme service
            _ = ThemeService.Instance;

            MainPage = new AppShell();
        }
    }
}