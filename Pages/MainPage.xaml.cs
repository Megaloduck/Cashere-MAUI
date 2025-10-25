using Cashere.Models;
using Cashere.PageModels;
using Cashere.Services; // ✅ Import ThemeService namespace

namespace Cashere.Pages
{
    public partial class MainPage : ContentPage
    {
        private readonly CashierPageModel _pageModel;

        public MainPage()
        {
            InitializeComponent();
            _pageModel = new CashierPageModel();
            BindingContext = _pageModel;
            UpdateThemeIcon();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _pageModel.InitializeAsync();
            UpdateThemeIcon();
        }

        private void OnThemeToggleClicked(object sender, EventArgs e)
        {
            ThemeService.Instance.IsDarkMode = !ThemeService.Instance.IsDarkMode;
            UpdateThemeIcon();
        }

        private void UpdateThemeIcon()
        {
            if (ThemeToggleButton != null)
            {
                ThemeToggleButton.Text = ThemeService.Instance.IsDarkMode ? "☀️" : "🌙";
            }
        }

        private void OnMenuItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() != null)
            {
                ((CollectionView)sender).SelectedItem = null;
            }
        }
        
        private void OnCartItemQuantityChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry && entry.BindingContext is CartItemModel cartItem)
            {
                if (int.TryParse(e.NewTextValue, out int quantity) && quantity > 0)
                {
                    cartItem.Quantity = quantity;
                    cartItem.RecalculateTotal();
                    _pageModel.RecalculateCartTotals();
                }
            }
        }

 
    }
}
