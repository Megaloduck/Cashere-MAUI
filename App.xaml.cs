using Cashere.Pages;
using Microsoft.Maui.Devices;

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

            // Run after shell is ready
            Dispatcher.Dispatch(async () =>
            {
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    // Windows PC: host the API
                    await Shell.Current.GoToAsync("//serversetup");
                }
                else if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    // Android device: go to login
                    await Shell.Current.GoToAsync("//login");
                }
                else
                {
                    // Fallback (iOS/Mac)
                    await Shell.Current.GoToAsync("//login");
                }
            });
        }
    }
}