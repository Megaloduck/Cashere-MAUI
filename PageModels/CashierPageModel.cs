using Cashere.Models;
using Cashere.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Cashere.PageModels
{
    public class CashierPageModel : BasePageModel
    {
        private readonly ApiService _apiService;
        private ObservableCollection<MenuCategoryModel> _categories;
        private ObservableCollection<MenuItemModel> _filteredMenuItems;
        private ObservableCollection<CartItemModel> _cartItems;
        private MenuCategoryModel _selectedCategory;
        private decimal _cartSubtotal;
        private decimal _cartTax;
        private decimal _cartTotal;
        private decimal _taxRate = 0.10m;

        public ObservableCollection<MenuCategoryModel> Categories
        {
            get => _categories;
            set { _categories = value; OnPropertyChanged(); }
        }

        public ObservableCollection<MenuItemModel> FilteredMenuItems
        {
            get => _filteredMenuItems;
            set { _filteredMenuItems = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CartItemModel> CartItems
        {
            get => _cartItems;
            set { _cartItems = value; OnPropertyChanged(); }
        }

        public MenuCategoryModel SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); }
        }

        public decimal CartSubtotal
        {
            get => _cartSubtotal;
            set { _cartSubtotal = value; OnPropertyChanged(); }
        }

        public decimal CartTax
        {
            get => _cartTax;
            set { _cartTax = value; OnPropertyChanged(); }
        }

        public decimal CartTotal
        {
            get => _cartTotal;
            set { _cartTotal = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand SelectCategoryCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand OpenAdminPanelCommand { get; }

        public CashierPageModel()
        {
            _apiService = new ApiService();
            Categories = new ObservableCollection<MenuCategoryModel>();
            FilteredMenuItems = new ObservableCollection<MenuItemModel>();
            CartItems = new ObservableCollection<CartItemModel>();

            SelectCategoryCommand = new Command<MenuCategoryModel>(OnSelectCategory);
            AddToCartCommand = new Command<MenuItemModel>(OnAddToCart);
            RemoveFromCartCommand = new Command<CartItemModel>(OnRemoveFromCart);
            IncreaseQuantityCommand = new Command<CartItemModel>(OnIncreaseQuantity);
            DecreaseQuantityCommand = new Command<CartItemModel>(OnDecreaseQuantity);
            ClearCartCommand = new Command(OnClearCart);
            CheckoutCommand = new Command(OnCheckout);
            OpenAdminPanelCommand = new Command(OnOpenAdminPanel);
        }

        private async void OnOpenAdminPanel()
        {
            await Shell.Current.GoToAsync("admin");
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;

                var categories = await _apiService.GetMenuCategoriesAsync();

                // 🧹 Fix: clear the collection to prevent duplication
                Categories.Clear();

                foreach (var category in categories)
                {
                    Categories.Add(new MenuCategoryModel
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Description = category.Description,
                        DisplayOrder = category.DisplayOrder,
                        Items = new ObservableCollection<MenuItemModel>(
                            category.Items.Select(i => new MenuItemModel
                            {
                                Id = i.Id,
                                Name = i.Name,
                                Description = i.Description,
                                Price = i.Price,
                                IsTaxable = i.IsTaxable,
                                DisplayOrder = i.DisplayOrder,
                                Category = i.Name
                            }).ToList()
                        )
                    });
                }

                if (Categories.Any())
                {
                    SelectedCategory = Categories.First();
                    OnSelectCategory(SelectedCategory);
                }

                var taxSettings = await _apiService.GetTaxSettingsAsync();
                _taxRate = taxSettings.DefaultTaxRate;
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load menu: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnSelectCategory(MenuCategoryModel category)
        {
            if (category == null) return;

            SelectedCategory = category;

            // Update filtered items
            FilteredMenuItems.Clear();
            foreach (var item in category.Items.OrderBy(i => i.DisplayOrder))
            {
                FilteredMenuItems.Add(item);
            }
        }

        private void OnAddToCart(MenuItemModel item)
        {
            if (item == null) return;

            var existingItem = CartItems.FirstOrDefault(ci => ci.MenuItemId == item.Id);

            if (existingItem != null)
            {
                existingItem.Quantity++;
                existingItem.RecalculateTotal();
            }
            else
            {
                CartItems.Add(new CartItemModel
                {
                    MenuItemId = item.Id,
                    ItemName = item.Name,
                    UnitPrice = item.Price,
                    Quantity = 1,
                    IsTaxable = item.IsTaxable,
                    TaxRate = item.IsTaxable ? _taxRate : 0
                });
                CartItems.Last().RecalculateTotal();
            }

            RecalculateCartTotals();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current!.MainPage!.DisplayAlert("Added", $"{item.Name} added to cart", "OK");
            });
        }

        private void OnRemoveFromCart(CartItemModel item)
        {
            if (item != null)
            {
                CartItems.Remove(item);
                RecalculateCartTotals();
            }
        }

        private void OnIncreaseQuantity(CartItemModel item)
        {
            if (item != null)
            {
                item.Quantity++;
                item.RecalculateTotal();
                RecalculateCartTotals();
            }
        }

        private void OnDecreaseQuantity(CartItemModel item)
        {
            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
                item.RecalculateTotal();
                RecalculateCartTotals();
            }
        }

        private void OnClearCart()
        {
            CartItems.Clear();
            RecalculateCartTotals();
        }

        private async void OnCheckout()
        {
            if (!CartItems.Any())
            {
                await Application.Current!.MainPage!.DisplayAlert("Empty Cart", "Please add items before checkout", "OK");
                return;
            }

            // Navigate to checkout page
            await Shell.Current.GoToAsync("checkout");
        }

        public void RecalculateCartTotals()
        {
            CartSubtotal = CartItems.Sum(ci => ci.SubtotalAmount);
            CartTax = CartItems.Sum(ci => ci.TaxAmount);
            CartTotal = CartSubtotal + CartTax;
        }
    }

    public class BasePageModel : INotifyPropertyChanged
    {
        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}