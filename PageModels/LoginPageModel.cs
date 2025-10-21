using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cashere.Models;
using Cashere.Services;
using System.Windows.Input;

namespace Cashere.PageModels
{
    public class LoginPageModel : BasePageModel
    {
        private readonly ApiService _apiService;
        private string _username;
        private string _password;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        public LoginPageModel()
        {
            _apiService = new ApiService();
            LoginCommand = new Command(OnLogin);

            // Default credentials for testing
            Username = "admin";
            Password = "admin123";
        }

        private async void OnLogin()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "Please enter username and password", "OK");
                return;
            }

            try
            {
                IsLoading = true;

                var response = await _apiService.LoginAsync(Username, Password);

                await Application.Current!.MainPage!.DisplayAlert("Success", $"Welcome {response.Username}!", "OK");

                // Navigate to main cashier page
                await Shell.Current.GoToAsync("//main");
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Login Failed", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}