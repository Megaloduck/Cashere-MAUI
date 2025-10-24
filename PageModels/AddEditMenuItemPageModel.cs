using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Cashere.Models;
using Cashere.Services;

namespace Cashere.PageModels
{
    public class AddEditMenuItemPageModel : BasePageModel, IQueryAttributable
    {
        private readonly ApiService _apiService;
        private MenuItemModel _itemToEdit;   
        private bool _isEditMode;

        private string _itemName;
        private string _description;
        private string _price;
        private int _displayOrder = 1;
        private bool _isTaxable = true;
        private bool _hasCustomTaxRate;
        private string _customTaxRate;
        private bool _isActive = true;
        private CategoryModel _selectedCategory;
        private ObservableCollection<CategoryModel> _categories;

        public string PageTitle => _isEditMode ? "Edit Menu Item" : "Add Menu Item";
        public string SaveButtonText => _isEditMode ? "💾 Update Item" : "➕ Add Item";

        public string ItemName
        {
            get => _itemName;
            set { _itemName = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public string Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        public int DisplayOrder
        {
            get => _displayOrder;
            set { _displayOrder = value; OnPropertyChanged(); }
        }

        public bool IsTaxable
        {
            get => _isTaxable;
            set { _isTaxable = value; OnPropertyChanged(); }
        }

        public bool HasCustomTaxRate
        {
            get => _hasCustomTaxRate;
            set { _hasCustomTaxRate = value; OnPropertyChanged(); }
        }

        public string CustomTaxRate
        {
            get => _customTaxRate;
            set { _customTaxRate = value; OnPropertyChanged(); }
        }

        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        public CategoryModel SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CategoryModel> Categories
        {
            get => _categories;
            set { _categories = value; OnPropertyChanged(); }
        }

        public ICommand IncreaseOrderCommand { get; }
        public ICommand DecreaseOrderCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddEditMenuItemPageModel(MenuItemModel itemToEdit = null)
        {
            _apiService = new ApiService();
            _itemToEdit = itemToEdit;
            _isEditMode = itemToEdit != null;
            Categories = new ObservableCollection<CategoryModel>();

            IncreaseOrderCommand = new Command(() => DisplayOrder++);
            DecreaseOrderCommand = new Command(() => { if (DisplayOrder > 1) DisplayOrder--; });
            SaveCommand = new Command(OnSave);
            CancelCommand = new Command(OnCancel);

            // If editing, populate fields
           
        }
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("menuItem"))
            {
                _itemToEdit = query["menuItem"] as MenuItemModel;
                _isEditMode = _itemToEdit != null;

                if (_isEditMode)
                {
                    // Populate fields with existing item data
                    ItemName = _itemToEdit.Name;
                    Description = _itemToEdit.Description;
                    Price = _itemToEdit.Price.ToString("F0");
                    DisplayOrder = _itemToEdit.DisplayOrder;
                    IsTaxable = _itemToEdit.IsTaxable;
                    IsActive = true; // or get from model if you track it

                    // Update page title
                    OnPropertyChanged(nameof(PageTitle));
                    OnPropertyChanged(nameof(SaveButtonText));
                }
            }
            else
            {
                // No parameter = Add mode
                _isEditMode = false;
                _itemToEdit = null;
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;

                // Load categories
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

                // If editing, select the category
                if (_isEditMode && _itemToEdit != null)
                {
                    // Select the category that matches the item
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == _itemToEdit.CategoryId);

                    // If custom tax rate exists, populate it
                    if (_itemToEdit.CustomTaxRate.HasValue)
                    {
                        HasCustomTaxRate = true;
                        CustomTaxRate = (_itemToEdit.CustomTaxRate.Value * 100).ToString("F2"); // Convert to percentage
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load categories: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void OnSave()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(ItemName))
            {
                await Application.Current!.MainPage!.DisplayAlert("Validation", "Please enter item name", "OK");
                return;
            }

            if (SelectedCategory == null)
            {
                await Application.Current!.MainPage!.DisplayAlert("Validation", "Please select a category", "OK");
                return;
            }

            if (!decimal.TryParse(Price, out decimal priceValue) || priceValue <= 0)
            {
                await Application.Current!.MainPage!.DisplayAlert("Validation", "Please enter a valid price", "OK");
                return;
            }

            decimal? customTaxRateValue = null;
            if (HasCustomTaxRate && IsTaxable)
            {
                if (decimal.TryParse(CustomTaxRate, out decimal rate))
                {
                    customTaxRateValue = rate / 100; // Convert percentage to decimal
                }
            }

            try
            {
                IsLoading = true;

                if (_isEditMode)
                {
                    // Update existing item
                    await _apiService.UpdateMenuItemAsync(
                        _itemToEdit.Id,
                        SelectedCategory.Id,
                        ItemName,
                        Description,
                        priceValue,
                        IsTaxable,
                        customTaxRateValue,
                        IsActive,
                        DisplayOrder
                    );

                    await Application.Current!.MainPage!.DisplayAlert("Success", "Menu item updated!", "OK");
                }
                else
                {
                    // Create new item
                    await _apiService.CreateMenuItemAsync(
                        SelectedCategory.Id,
                        ItemName,
                        Description,
                        priceValue,
                        IsTaxable,
                        customTaxRateValue,
                        DisplayOrder
                    );

                    await Application.Current!.MainPage!.DisplayAlert("Success", "Menu item added!", "OK");
                }

                // Go back
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void OnCancel()
        {
            // Optional: Ask for confirmation if user has made changes
            bool hasChanges = !string.IsNullOrEmpty(ItemName) ||
                              !string.IsNullOrEmpty(Description) ||
                              !string.IsNullOrEmpty(Price);

            if (hasChanges && !_isEditMode)
            {
                bool confirm = await Application.Current!.MainPage!.DisplayAlert(
                    "Discard Changes?",
                    "You have unsaved changes. Are you sure you want to go back?",
                    "Yes, Discard",
                    "No, Stay");

                if (!confirm)
                    return; // User chose to stay
            }

            await Shell.Current.GoToAsync("..");
        }
    }
}