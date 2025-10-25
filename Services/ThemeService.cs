using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage; // for Preferences
using Cashere.Resources.Themes;
using System.ComponentModel;

namespace Cashere.Services
{
    public class ThemeService : INotifyPropertyChanged
    {
        private static ThemeService _instance;
        public static ThemeService Instance => _instance ??= new ThemeService();

        private bool _isDarkMode;
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    OnPropertyChanged(nameof(IsDarkMode));
                    ApplyTheme();
                    SaveThemePreference();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ThemeService()
        {
            LoadThemePreference();
            ApplyTheme();
        }

        private void LoadThemePreference()
        {
            _isDarkMode = Preferences.Get("IsDarkMode", false);
        }

        private void SaveThemePreference()
        {
            Preferences.Set("IsDarkMode", _isDarkMode);
        }

        private void ApplyTheme()
        {
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            mergedDictionaries.Clear();

            if (_isDarkMode)
            {
                mergedDictionaries.Add(new DarkTheme());
            }
            else
            {
                mergedDictionaries.Add(new LightTheme());
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}