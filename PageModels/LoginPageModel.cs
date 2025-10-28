using Cashere.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Storage;

namespace Cashere.PageModels
{
    public class LoginPageModel : BasePageModel
    {
        private readonly ApiService _apiService;
        private string _username;
        private string _password;
        private bool _isLoading;
        private bool _rememberMe;
        private bool _isPasswordHidden = true;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotLoading)); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotLoading)); }
        }


        public bool IsNotLoading => !IsLoading && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

        public bool RememberMe
        {
            get => _rememberMe;
            set { _rememberMe = value; OnPropertyChanged(); }
        }

        public bool IsPasswordHidden
        {
            get => _isPasswordHidden;
            set { _isPasswordHidden = value; OnPropertyChanged(); }
        }

        public string ThemeIcon => ThemeService.Instance.IsDarkMode ? "☀️" : "🌙";

        public ICommand LoginCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand QuickLoginCommand { get; }
        public ICommand ContactAdminCommand { get; }
        public ICommand ToggleThemeCommand { get; }

        public LoginPageModel()
        {
            _apiService = new ApiService();

            LoginCommand = new Command(OnLogin);
            TogglePasswordVisibilityCommand = new Command(OnTogglePasswordVisibility);
            ForgotPasswordCommand = new Command(OnForgotPassword);
            QuickLoginCommand = new Command<string>(OnQuickLogin);
            ContactAdminCommand = new Command(OnContactAdmin);
            ToggleThemeCommand = new Command(OnToggleTheme);

            ThemeService.Instance.PropertyChanged += OnThemeChanged;

            LoadSavedCredentials();
        }

        private void LoadSavedCredentials()
        {
            RememberMe = Preferences.Get("RememberMe", false);
            if (RememberMe)
            {
                Username = Preferences.Get("SavedUsername", string.Empty);
            }
        }

        private void OnThemeChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ThemeService.IsDarkMode))
                OnPropertyChanged(nameof(ThemeIcon));
        }

        private void OnToggleTheme()
        {
            ThemeService.Instance.IsDarkMode = !ThemeService.Instance.IsDarkMode;
        }

        private async void OnLogin()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Invalid Input",
                    "Please enter both username and password",
                    "OK");
                return;
            }

            try
            {
                IsLoading = true;

                var response = await _apiService.LoginAsync(Username, Password);

                if (RememberMe)
                {
                    Preferences.Set("RememberMe", true);
                    Preferences.Set("SavedUsername", Username);
                }
                else
                {
                    Preferences.Remove("RememberMe");
                    Preferences.Remove("SavedUsername");
                }

                await SecureStorage.SetAsync("auth_token", response.Token);
                await SecureStorage.SetAsync("user_role", response.Role);

                await Application.Current!.MainPage!.DisplayAlert("Success", $"Welcome {response.Username}!", "OK");
                await Shell.Current.GoToAsync("//main");
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Login Failed",
                    ex.Message,
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnTogglePasswordVisibility() => IsPasswordHidden = !IsPasswordHidden;

        private async void OnForgotPassword()
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Forgot Password",
                "Please contact your administrator to reset your password.",
                "OK");
        }

        private async void OnQuickLogin(string mode)
        {
            if (mode == "demo")
            {
                Username = "demo";
                Password = "demo123";
            }
            else if (mode == "guest")
            {
                Username = "guest";
                Password = "guest123";
            }

            await Task.Delay(100);
            OnLogin();
        }

        private async void OnContactAdmin()
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Contact Administrator",
                "Please contact your system administrator at:\n\nadmin@cashere.com\n+62 123 456 7890",
                "OK");
        }
    }
}
