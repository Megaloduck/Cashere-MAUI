using Cashere.Models;
using Cashere.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Cashere.PageModels
{
    public class AdminPanelPageModel : BasePageModel
    {
        private readonly ApiService _apiService;
        private string _selectedTab = "Dashboard";

        // Tab visibility properties
        public bool IsDashboardTab => _selectedTab == "Dashboard";
        public bool IsCategoriesTab => _selectedTab == "Categories";
        public bool IsMenuItemsTab => _selectedTab == "MenuItems";
        public bool IsTaxSettingsTab => _selectedTab == "TaxSettings";
        public bool IsUsersTab => _selectedTab == "Users";

        // Dashboard properties
        private decimal _todayRevenue;
        private int _todayTransactions;
        private decimal _todayCash;
        private decimal _todayQRIS;
        private ObservableCollection<TransactionSummary> _recentTransactions;

        public decimal TodayRevenue
        {
            get => _todayRevenue;
            set { _todayRevenue = value; OnPropertyChanged(); }
        }

        public int TodayTransactions
        {
            get => _todayTransactions;
            set { _todayTransactions = value; OnPropertyChanged(); }
        }

        public decimal TodayCash
        {
            get => _todayCash;
            set { _todayCash = value; OnPropertyChanged(); }
        }

        public decimal TodayQRIS
        {
            get => _todayQRIS;
            set { _todayQRIS = value; OnPropertyChanged(); }
        }

        public ObservableCollection<TransactionSummary> RecentTransactions
        {
            get => _recentTransactions;
            set { _recentTransactions = value; OnPropertyChanged(); }
        }

        // Categories properties
        private ObservableCollection<CategoryModel> _categories;
        public ObservableCollection<CategoryModel> Categories
        {
            get => _categories;
            set { _categories = value; OnPropertyChanged(); }
        }

        // Menu Items properties
        private ObservableCollection<MenuItemModel> _menuItems;
        public ObservableCollection<MenuItemModel> MenuItems
        {
            get => _menuItems;
            set { _menuItems = value; OnPropertyChanged(); }
        }

        // Tax Settings properties
        private string _taxName;
        private decimal _taxRate;
        private bool _isTaxEnabled;

        public string TaxName
        {
            get => _taxName;
            set { _taxName = value; OnPropertyChanged(); }
        }

        public decimal TaxRate
        {
            get => _taxRate;
            set { _taxRate = value; OnPropertyChanged(); }
        }

        public bool IsTaxEnabled
        {
            get => _isTaxEnabled;
            set { _isTaxEnabled = value; OnPropertyChanged(); }
        }

        // Users properties
        private ObservableCollection<UserModel> _users;
        public ObservableCollection<UserModel> Users
        {
            get => _users;
            set { _users = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand SelectTabCommand { get; }
        public ICommand BackToCashierCommand { get; }

        // Category commands
        public ICommand AddCategoryCommand { get; }
        public ICommand EditCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }

        // Menu Item commands
        public ICommand AddMenuItemCommand { get; }
        public ICommand EditMenuItemCommand { get; }
        public ICommand DeleteMenuItemCommand { get; }

        // Tax commands
        public ICommand SaveTaxSettingsCommand { get; }

        // User commands
        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand OpenDashboardCommand { get; }

        public AdminPanelPageModel()
        {
            _apiService = new ApiService();
            RecentTransactions = new ObservableCollection<TransactionSummary>();
            Categories = new ObservableCollection<CategoryModel>();
            MenuItems = new ObservableCollection<MenuItemModel>();
            Users = new ObservableCollection<UserModel>();

            SelectTabCommand = new Command<string>(async tab => await OnSelectTabAsync(tab));
            BackToCashierCommand = new Command(async () => await OnBackToCashierAsync());

            AddCategoryCommand = new Command(async () => await OnAddCategoryAsync());
            EditCategoryCommand = new Command<CategoryModel>(async cat => await OnEditCategoryAsync(cat));
            DeleteCategoryCommand = new Command<CategoryModel>(async cat => await OnDeleteCategoryAsync(cat));

            AddMenuItemCommand = new Command(async () => await OnAddMenuItemAsync());
            EditMenuItemCommand = new Command<MenuItemModel>(async item => await OnEditMenuItemAsync(item));
            DeleteMenuItemCommand = new Command<MenuItemModel>(async item => await OnDeleteMenuItemAsync(item));

            SaveTaxSettingsCommand = new Command(async () => await OnSaveTaxSettingsAsync());

            AddUserCommand = new Command(async () => await OnAddUserAsync());
            EditUserCommand = new Command<UserModel>(async user => await OnEditUserAsync(user));
            DeleteUserCommand = new Command<UserModel>(async user => await OnDeleteUserAsync(user));
            ResetPasswordCommand = new Command<UserModel>(async user => await OnResetPasswordAsync(user));

            OpenDashboardCommand = new Command(async () => await Shell.Current.GoToAsync("dashboard"));
        }

        public async Task InitializeAsync()
        {
            await LoadDashboardDataAsync();
        }

        private async Task OnSelectTabAsync(string tabName)
        {
            _selectedTab = tabName;
            OnPropertyChanged(nameof(IsDashboardTab));
            OnPropertyChanged(nameof(IsCategoriesTab));
            OnPropertyChanged(nameof(IsMenuItemsTab));
            OnPropertyChanged(nameof(IsTaxSettingsTab));
            OnPropertyChanged(nameof(IsUsersTab));

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                switch (tabName)
                {
                    case "Dashboard":
                        await LoadDashboardDataAsync();
                        break;
                    case "Categories":
                        await LoadCategoriesAsync();
                        break;
                    case "MenuItems":
                        await LoadMenuItemsAsync();
                        break;
                    case "TaxSettings":
                        await LoadTaxSettingsAsync();
                        break;
                    case "Users":
                        await LoadUsersAsync();
                        break;
                }
            });
        }

        private async Task OnBackToCashierAsync()
        {
            await Shell.Current.GoToAsync("//main");
        }

        // ============ DASHBOARD ============
        private async Task LoadDashboardDataAsync()
        {
            try
            {
                IsLoading = true;

                var summary = await _apiService.GetTodaySummaryAsync();
                TodayRevenue = summary.TotalRevenue;
                TodayTransactions = summary.TotalTransactions;
                TodayCash = summary.CashCollected;
                TodayQRIS = summary.QRISCollected;

                var transactions = await _apiService.GetRecentTransactionsAsync(10);
                RecentTransactions.Clear();
                foreach (var t in transactions)
                {
                    RecentTransactions.Add(new TransactionSummary
                    {
                        OrderNumber = t.OrderNumber,
                        PaymentMethod = t.PaymentMethod,
                        OrderTotal = t.OrderTotal,
                        TransactionDate = t.TransactionDate
                    });
                }
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load dashboard: {ex.Message}", "OK"));
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ============ USERS ============
        private async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;
                var users = await _apiService.GetAllUsersAsync();

                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(new UserModel
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        IsActive = user.IsActive
                    });
                }
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load users: {ex.Message}", "OK"));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnAddUserAsync()
        {
            string username = await Application.Current!.MainPage!.DisplayPromptAsync("Add User", "Username:");
            if (string.IsNullOrEmpty(username)) return;

            string email = await Application.Current!.MainPage!.DisplayPromptAsync("Add User", "Email:");
            string password = await Application.Current!.MainPage!.DisplayPromptAsync("Add User", "Password:");

            string role = await Application.Current!.MainPage!.DisplayActionSheet("Select Role", "Cancel", null, "Cashier", "FinanceOfficer", "Admin");
            if (role == "Cancel") return;

            try
            {
                IsLoading = true;
                await _apiService.CreateUserAsync(username, email, password, role);
                await LoadUsersAsync();
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Success", "User created!", "OK"));
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Error", ex.Message, "OK"));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnEditUserAsync(UserModel user)
        {
            string username = await Application.Current!.MainPage!.DisplayPromptAsync("Edit User", "Username:", initialValue: user.Username);
            if (string.IsNullOrEmpty(username)) return;

            string email = await Application.Current!.MainPage!.DisplayPromptAsync("Edit User", "Email:", initialValue: user.Email);
            string role = await Application.Current!.MainPage!.DisplayActionSheet("Select Role", "Cancel", null, "Cashier", "FinanceOfficer", "Admin");
            if (role == "Cancel") return;

            try
            {
                IsLoading = true;
                await _apiService.UpdateUserAsync(user.Id, username, email, role, user.IsActive);
                await LoadUsersAsync();
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Success", "User updated!", "OK"));
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Error", ex.Message, "OK"));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnDeleteUserAsync(UserModel user)
        {
            bool confirm = await Application.Current!.MainPage!.DisplayAlert("Confirm", $"Delete user '{user.Username}'?", "Yes", "No");
            if (!confirm) return;

            try
            {
                IsLoading = true;
                await _apiService.DeleteUserAsync(user.Id);
                await LoadUsersAsync();
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Success", "User deleted!", "OK"));
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Error", ex.Message, "OK"));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnResetPasswordAsync(UserModel user)
        {
            string newPassword = await Application.Current!.MainPage!.DisplayPromptAsync("Reset Password", $"New password for {user.Username}:");
            if (string.IsNullOrEmpty(newPassword)) return;

            try
            {
                IsLoading = true;
                await _apiService.ResetUserPasswordAsync(user.Id, newPassword);
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Success", "Password reset!", "OK"));
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Error", ex.Message, "OK"));
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ============ CATEGORIES ============
        private async Task LoadCategoriesAsync()
        {
            try
            {
                IsLoading = true;
                var categories = await _apiService.GetMenuCategoriesAsync();

                Categories.Clear();
                foreach (var cat in categories)
                {
                    Categories.Add(new CategoryModel
                    {
                        Id = cat.Id,
                        Name = cat.Name,
                        Description = cat.Description,
                        DisplayOrder = cat.DisplayOrder
                    });
                }
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load categories: {ex.Message}", "OK"));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnAddCategoryAsync()
        {
            string name = await Application.Current!.MainPage!.DisplayPromptAsync("Add Category", "Category Name:");
            if (string.IsNullOrEmpty(name)) return;

            string description = await Application.Current!.MainPage!.DisplayPromptAsync("Add Category", "Description:");

            try
            {
                IsLoading = true;
                await _apiService.CreateCategoryAsync(name, description, Categories.Count + 1);
                await LoadCategoriesAsync();
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Success", "Category added!", "OK"));
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Error", ex.Message, "OK"));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnEditCategoryAsync(CategoryModel category)
        {
            string name = await Application.Current!.MainPage!.DisplayPromptAsync("Edit Category", "Name:", initialValue: category.Name);
            if (string.IsNullOrEmpty(name)) return;

            string description = await Application.Current!.MainPage!.DisplayPromptAsync("Edit Category", "Description:", initialValue: category.Description);

            try
            {
                IsLoading = true;
                await _apiService.UpdateCategoryAsync(category.Id, name, description, category.DisplayOrder);
                await LoadCategoriesAsync();
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Success", "Category updated!", "OK"));
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Error", ex.Message, "OK"));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnDeleteCategoryAsync(CategoryModel category)
        {
            bool confirm = await Application.Current!.MainPage!.DisplayAlert("Confirm", $"Delete category '{category.Name}'?", "Yes", "No");
            if (!confirm) return;

            try
            {
                IsLoading = true;
                await _apiService.DeleteCategoryAsync(category.Id);
                await LoadCategoriesAsync();
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Success", "Category deleted!", "OK"));
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Error", ex.Message, "OK"));
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ============ MENU ITEMS ============
        private async Task LoadMenuItemsAsync()
        {
            try
            {
                IsLoading = true;
                var categories = await _apiService.GetMenuCategoriesAsync();

                MenuItems.Clear();
                foreach (var cat in categories)
                {
                    if (cat.Items == null) continue;
                    foreach (var item in cat.Items)
                    {
                        MenuItems.Add(new MenuItemModel
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Description = item.Description,
                            Price = item.Price,
                            IsTaxable = item.IsTaxable
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load menu items: {ex.Message}", "OK"));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnAddMenuItemAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                Application.Current!.MainPage!.DisplayAlert("Add Item", "Use Category selection and item form (implement full form)", "OK"));
            // TODO: Implement full form with category dropdown, price, etc.
        }

        private async Task OnEditMenuItemAsync(MenuItemModel item)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                Application.Current!.MainPage!.DisplayAlert("Edit Item", $"Edit {item.Name} (implement full form)", "OK"));
            // TODO: Implement full editing form
        }

        private async Task OnDeleteMenuItemAsync(MenuItemModel item)
        {
            bool confirm = await Application.Current!.MainPage!.DisplayAlert("Confirm", $"Delete '{item.Name}'?", "Yes", "No");
            if (!confirm) return;

            try
            {
                IsLoading = true;
                await _apiService.DeleteMenuItemAsync(item.Id);
                await LoadMenuItemsAsync();
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Success", "Item deleted!", "OK"));
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Error", ex.Message, "OK"));
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ============ TAX SETTINGS ============
        private async Task LoadTaxSettingsAsync()
        {
            try
            {
                IsLoading = true;
                var settings = await _apiService.GetTaxSettingsAsync();

                TaxName = settings.TaxName;
                TaxRate = settings.DefaultTaxRate * 100; // Convert to percentage
                IsTaxEnabled = settings.IsEnabled;
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load tax settings: {ex.Message}", "OK"));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnSaveTaxSettingsAsync()
        {
            try
            {
                IsLoading = true;
                await _apiService.UpdateTaxSettingsAsync(TaxName, TaxRate / 100, IsTaxEnabled); // Convert back to decimal
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Success", "Tax settings saved!", "OK"));
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlert("Error", ex.Message, "OK"));
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    // Supporting Models (you can move these into separate files under Models/ later)
    public class TransactionSummary
    {
        public string OrderNumber { get; set; }
        public string PaymentMethod { get; set; }
        public decimal OrderTotal { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    public class CategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }

        // Assume API returns Items collection; keep it optional
        public ObservableCollection<MenuItemModel> Items { get; set; }
    }

    //public class MenuItemModel
    //{
       // public int Id { get; set; }
        //public string Name { get; set; }
        //public string Description { get; set; }
        //public decimal Price { get; set; }
        //public bool IsTaxable { get; set; }
    //}

    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
    }
}
