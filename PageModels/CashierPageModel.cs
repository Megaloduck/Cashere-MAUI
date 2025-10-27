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
        private ObservableCollection<MenuItemModel> _menuItems;
        private ObservableCollection<MenuItemModel> _filteredMenuItems;
        private ObservableCollection<CartItemModel> _cartItems;
        private MenuCategoryModel _selectedCategory;
        private string _searchText;
        private bool _isSearchActive;
        private decimal _cartSubtotal;
        private decimal _cartTax;
        private decimal _cartTotal;
        private decimal _taxRate = 0.10m;

        public string Today
        {
            get => DateTime.Now.ToString("dddd, dd MMMM yyyy");
        }
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                PerformSearch();
            }
        }

        public bool IsSearchActive
        {
            get => _isSearchActive;
            set
            {
                _isSearchActive = value;
                OnPropertyChanged();
            }
        }
        public bool HasNoSearchResults => IsSearchActive && !FilteredMenuItems.Any();

        public ObservableCollection<MenuCategoryModel> Categories
        {
            get => _categories;
            set { _categories = value; OnPropertyChanged(); }
        }

        public ObservableCollection<MenuItemModel> MenuItems
        {
            get => _menuItems;
            set { _menuItems = value; OnPropertyChanged(); }
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
        public ICommand ToggleThemeCommand { get; }
        public ICommand SelectCategoryCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand OpenAdminPanelCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand QuickSearchCommand { get; }

        public CashierPageModel()
        {
            _apiService = new ApiService();
            ToggleThemeCommand = new Command(OnToggleTheme);

            ClearSearchCommand = new Command(OnClearSearch);
            SearchCommand = new Command(PerformSearch);

            Categories = new ObservableCollection<MenuCategoryModel>();
            MenuItems = new ObservableCollection<MenuItemModel>();
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

        private void OnToggleTheme()
        {
            ThemeService.Instance.IsDarkMode = !ThemeService.Instance.IsDarkMode;
        }

        private async void OnOpenAdminPanel()
        {
            await Shell.Current.GoToAsync("admin");
        }
        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Show all items from selected category
                IsSearchActive = false;
                LoadFilteredMenuItems();
                return;
            }

            IsSearchActive = true;

            var searchLower = SearchText.ToLower().Trim();

            // Search across all menu items, ignoring category filter
            var searchResults = MenuItems
                .Where(item => 
                    item.Name.ToLower().Contains(searchLower) ||
                    item.Description.ToLower().Contains(searchLower) ||
                    item.Category.ToLower().Contains(searchLower))
                .ToList();

            FilteredMenuItems.Clear();
            foreach (var item in searchResults)
            {
                FilteredMenuItems.Add(item);
            }

            // Update UI message if no results
            if (!FilteredMenuItems.Any())
            {
                OnPropertyChanged(nameof(HasNoSearchResults));
            }
        }

        private void OnClearSearch()
        {
            SearchText = string.Empty;
            IsSearchActive = false;
            LoadFilteredMenuItems();
        }
        private void OnQuickSearch(string term)
        {
            SearchText = term;
        }

        private void LoadFilteredMenuItems()
        {
            FilteredMenuItems.Clear();

            if (SelectedCategory == null)
                return;

            var itemsToShow = SelectedCategory.Items.OrderBy(i => i.DisplayOrder);

            foreach (var item in itemsToShow)
            {
                FilteredMenuItems.Add(item);
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;

                var categories = await _apiService.GetMenuCategoriesAsync();

                // Clear collections to prevent duplication
                Categories.Clear();
                MenuItems.Clear();

                foreach (var category in categories)
                {
                    var categoryModel = new MenuCategoryModel
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Description = category.Description,
                        DisplayOrder = category.DisplayOrder,
                        Items = new ObservableCollection<MenuItemModel>()
                    };

                    // Add items to category and to global MenuItems list
                    foreach (var item in category.Items)
                    {
                        var menuItem = new MenuItemModel
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Description = item.Description,
                            Price = item.Price,
                            IsTaxable = item.IsTaxable,
                            DisplayOrder = item.DisplayOrder,
                            Category = category.Name // Use category name, not item name
                        };

                        categoryModel.Items.Add(menuItem);
                        MenuItems.Add(menuItem); // Add to global list for search
                    }

                    Categories.Add(categoryModel);
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

            // Clear search when selecting category
            if (IsSearchActive)
            {
                OnClearSearch();
            }

            // Deselect all categories
            foreach (var cat in Categories)
            {
                cat.IsSelected = false;
            }

            // Select the chosen category
            category.IsSelected = true;
            SelectedCategory = category;

            LoadFilteredMenuItems();
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
            try
            {
                if (!CartItems.Any())
                {
                    await Application.Current!.MainPage!.DisplayAlert("Empty Cart", "Please add items before checkout", "OK");
                    return;
                }

                // ✅ Safe navigation with crash logging
                await Shell.Current.Navigation.PushAsync(new CheckoutPage(CartItems.ToList()));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ OnCheckout crashed: " + ex);
                await Application.Current!.MainPage!.DisplayAlert("Error", ex.ToString(), "OK");
            }
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}