using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cashere.Models;
using Cashere.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Cashere.PageModels
{
    public class CheckoutPageModel : BasePageModel
    {
        private readonly ApiService _apiService;
        private ObservableCollection<CartItemModel> _cartItems;
        private string _orderNumber;
        private int _orderId;
        private decimal _subtotal;
        private decimal _tax;
        private decimal _total;
        private bool _isCashSelected = true;
        private bool _isQRISSelected;
        private decimal _cashAmountPaid;
        private decimal _change;

        public ObservableCollection<CartItemModel> CartItems
        {
            get => _cartItems;
            set { _cartItems = value; OnPropertyChanged(); }
        }

        public string OrderNumber
        {
            get => _orderNumber;
            set { _orderNumber = value; OnPropertyChanged(); }
        }

        public decimal Subtotal
        {
            get => _subtotal;
            set { _subtotal = value; OnPropertyChanged(); }
        }

        public decimal Tax
        {
            get => _tax;
            set { _tax = value; OnPropertyChanged(); }
        }

        public decimal Total
        {
            get => _total;
            set { _total = value; OnPropertyChanged(); }
        }

        public bool IsCashSelected
        {
            get => _isCashSelected;
            set { _isCashSelected = value; _isQRISSelected = !value; OnPropertyChanged(); OnPropertyChanged(nameof(IsQRISSelected)); }
        }

        public bool IsQRISSelected
        {
            get => _isQRISSelected;
            set { _isQRISSelected = value; _isCashSelected = !value; OnPropertyChanged(); OnPropertyChanged(nameof(IsCashSelected)); }
        }

        public decimal CashAmountPaid
        {
            get => _cashAmountPaid;
            set
            {
                _cashAmountPaid = value;
                Change = value - Total;
                OnPropertyChanged();
            }
        }

        public decimal Change
        {
            get => _change;
            set { _change = value; OnPropertyChanged(); }
        }

        public ICommand ProcessPaymentCommand { get; }
        public ICommand CancelCommand { get; }

        public CheckoutPageModel()
        {
            _apiService = new ApiService();
            CartItems = new ObservableCollection<CartItemModel>();
            ProcessPaymentCommand = new Command(OnProcessPayment);
            CancelCommand = new Command(OnCancel);
        }

        public async void InitializeAsync()
        {
            try
            {
                IsLoading = true;

                // Get cart data from main page (passed via navigation or static)
                // For now, we'll use a static property or messaging service
                // You'll need to implement passing cart data here

                // Create order first
                var orderRequest = new CreateOrderRequest
                {
                    Items = CartItems.Select(ci => new CreateOrderItemRequest
                    {
                        MenuItemId = ci.MenuItemId,
                        Quantity = ci.Quantity
                    }).ToList(),
                    DiscountAmount = 0
                };

                var orderResponse = await _apiService.CreateOrderAsync(orderRequest);
                OrderNumber = orderResponse.OrderNumber;
                _orderId = orderResponse.Id;
                Subtotal = orderResponse.SubtotalAmount;
                Tax = orderResponse.TaxAmount;
                Total = orderResponse.TotalAmount;
                CashAmountPaid = Total; // Default to exact amount
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to create order: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void OnProcessPayment()
        {
            try
            {
                IsLoading = true;

                // Validate payment
                if (IsCashSelected && CashAmountPaid < Total)
                {
                    await Application.Current!.MainPage!.DisplayAlert("Error", "Insufficient cash amount", "OK");
                    return;
                }

                // Process payment via API
                // TODO: Call payment API endpoint

                await Application.Current!.MainPage!.DisplayAlert("Success", "Payment processed successfully!", "OK");
                await Shell.Current.GoToAsync("//main");
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Payment failed: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void OnCancel()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}