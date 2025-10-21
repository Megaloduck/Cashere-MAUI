using Cashere.Models;
using Cashere.PageModels;

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
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _pageModel.InitializeAsync();
        }

        private async void OnMenuItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is MenuItemModel item)
            {
                _pageModel.AddToCartCommand.Execute(item);
                // Deselect after adding
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