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
    public class CheckoutPageModel : BasePageModel, IQueryAttributable
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
        private string _cashAmountPaid;
        private decimal _change;
        private bool _showChange;
        private bool _showInsufficientWarning;
        private bool _canProcessPayment;

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
            set { _total = value; OnPropertyChanged(); UpdateCanProcessPayment(); }
        }

        public bool IsCashSelected
        {
            get => _isCashSelected;
            set
            {
                _isCashSelected = value;
                if (value) _isQRISSelected = false;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsQRISSelected));
                UpdateCanProcessPayment();
            }
        }

        public bool IsQRISSelected
        {
            get => _isQRISSelected;
            set
            {
                _isQRISSelected = value;
                if (value) _isCashSelected = false;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCashSelected));
                UpdateCanProcessPayment();
            }
        }

        public string CashAmountPaid
        {
            get => _cashAmountPaid;
            set
            {
                _cashAmountPaid = value;
                OnPropertyChanged();
                CalculateChange();
            }
        }

        public decimal Change
        {
            get => _change;
            set { _change = value; OnPropertyChanged(); }
        }

        public bool ShowChange
        {
            get => _showChange;
            set { _showChange = value; OnPropertyChanged(); }
        }

        public bool ShowInsufficientWarning
        {
            get => _showInsufficientWarning;
            set { _showInsufficientWarning = value; OnPropertyChanged(); }
        }

        public bool CanProcessPayment
        {
            get => _canProcessPayment;
            set { _canProcessPayment = value; OnPropertyChanged(); }
        }

        public ICommand ProcessPaymentCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BackToCartCommand { get; }
        public ICommand SetExactAmountCommand { get; }
        public ICommand AddQuickAmountCommand { get; }
        public ICommand PreviewReceiptCommand { get; }
        public ICommand SelectPaymentMethodCommand { get; }

        public CheckoutPageModel(List<CartItemModel> cartItems = null)
        {
            _apiService = new ApiService();
            CartItems = new ObservableCollection<CartItemModel>(cartItems ?? new List<CartItemModel>());

            ProcessPaymentCommand = new Command(OnProcessPayment);
            CancelCommand = new Command(OnCancel);
            BackToCartCommand = new Command(OnBackToCart);
            SetExactAmountCommand = new Command(OnSetExactAmount);
            AddQuickAmountCommand = new Command<string>(OnAddQuickAmount);
            PreviewReceiptCommand = new Command(OnPreviewReceipt);
            SelectPaymentMethodCommand = new Command<string>(OnSelectPaymentMethod);
        }
        private void OnSelectPaymentMethod(string method)
        {
            if (method == "Cash")
            {
                IsCashSelected = true;
            }
            else if (method == "QRIS")
            {
                IsQRISSelected = true;
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // Receive cart data from cashier page
            if (query.ContainsKey("cartItems"))
            {
                var items = query["cartItems"] as List<CartItemModel>;
                if (items != null)
                {
                    CartItems.Clear();
                    foreach (var item in items)
                    {
                        CartItems.Add(item);
                    }
                }
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;

                // If cart is empty, get from static storage or navigation
                if (!CartItems.Any())
                {
                    // TODO: Get cart from messaging service or static storage
                    // For now, create a test order
                }

                // Calculate totals
                Subtotal = CartItems.Sum(ci => ci.SubtotalAmount);
                Tax = CartItems.Sum(ci => ci.TaxAmount);
                Total = Subtotal + Tax;

                // Create order
                await CreateOrderAsync();

                // Set default cash amount
                CashAmountPaid = Total.ToString("F0");
                CalculateChange();
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error",
                    $"Failed to initialize checkout: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CreateOrderAsync()
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create order: {ex.Message}");
            }
        }

        private void CalculateChange()
        {
            if (decimal.TryParse(CashAmountPaid, out decimal paid))
            {
                Change = paid - Total;
                ShowChange = Change >= 0;
                ShowInsufficientWarning = Change < 0;
                UpdateCanProcessPayment();
            }
            else
            {
                ShowChange = false;
                ShowInsufficientWarning = false;
                UpdateCanProcessPayment();
            }
        }

        private void UpdateCanProcessPayment()
        {
            if (IsQRISSelected)
            {
                CanProcessPayment = true; // QRIS always ready
            }
            else if (IsCashSelected)
            {
                if (decimal.TryParse(CashAmountPaid, out decimal paid))
                {
                    CanProcessPayment = paid >= Total;
                }
                else
                {
                    CanProcessPayment = false;
                }
            }
            else
            {
                CanProcessPayment = false;
            }
        }

        private void OnSetExactAmount()
        {
            CashAmountPaid = Total.ToString("F0");
        }

        private void OnAddQuickAmount(string amountStr)
        {
            if (int.TryParse(amountStr, out int quickAmount))
            {
                var currentAmount = decimal.TryParse(CashAmountPaid, out decimal current) ? current : 0;

                // Round up to nearest amount
                var roundedTotal = Math.Ceiling(Total / quickAmount) * quickAmount;
                CashAmountPaid = roundedTotal.ToString("F0");
            }
        }

        private async void OnProcessPayment()
        {
            if (!CanProcessPayment)
            {
                await Application.Current!.MainPage!.DisplayAlert("Invalid Payment",
                    "Please check your payment amount", "OK");
                return;
            }

            try
            {
                IsLoading = true;

                var paymentRequest = new ProcessPaymentRequest
                {
                    OrderId = _orderId,
                    PaymentMethod = IsCashSelected ? "Cash" : "QRIS",
                    AmountPaid = IsCashSelected
                        ? decimal.Parse(CashAmountPaid)
                        : Total
                };

                var paymentResponse = await _apiService.ProcessPaymentAsync(paymentRequest);

                // Show success message
                var message = IsCashSelected
                    ? $"Payment Successful!\n\nOrder: {paymentResponse.OrderNumber}\nTotal: Rp {paymentResponse.OrderTotal:N0}\nPaid: Rp {paymentResponse.AmountPaid:N0}\nChange: Rp {paymentResponse.ChangeAmount:N0}"
                    : $"QRIS Payment Successful!\n\nOrder: {paymentResponse.OrderNumber}\nTotal: Rp {paymentResponse.OrderTotal:N0}";

                await Application.Current!.MainPage!.DisplayAlert("✓ Success", message, "OK");

                // Print receipt (optional)
                var printReceipt = await Application.Current!.MainPage!.DisplayAlert(
                    "Print Receipt?",
                    "Do you want to print the receipt?",
                    "Yes",
                    "No");

                if (printReceipt)
                {
                    await PrintReceiptAsync(paymentResponse);
                }

                // Go back to main page and clear cart
                await Shell.Current.GoToAsync("//main");
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Payment Failed",
                    ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task PrintReceiptAsync(PaymentResponse payment)
        {
            try
            {
                // TODO: Implement actual printer integration
                // For now, just show a preview
                var receipt = GenerateReceiptText(payment);

                await Application.Current!.MainPage!.DisplayAlert(
                    "Receipt",
                    receipt,
                    "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Print error: {ex.Message}");
            }
        }

        private string GenerateReceiptText(PaymentResponse payment)
        {
            var receipt = "================================\n";
            receipt += "         CAFE POS RECEIPT        \n";
            receipt += "================================\n\n";
            receipt += $"Order #: {payment.OrderNumber}\n";
            receipt += $"Date: {payment.TransactionDate:dd/MM/yyyy HH:mm}\n";
            receipt += $"Payment: {payment.PaymentMethod}\n\n";
            receipt += "--------------------------------\n";
            receipt += "Items:\n";

            foreach (var item in CartItems)
            {
                receipt += $"{item.Quantity}x {item.ItemName}\n";
                receipt += $"   @ Rp {item.UnitPrice:N0} = Rp {item.SubtotalAmount:N0}\n";
            }

            receipt += "--------------------------------\n";
            receipt += $"Subtotal:  Rp {payment.OrderTotal - payment.TaxAmount:N0}\n";
            receipt += $"Tax (10%): Rp {payment.TaxAmount:N0}\n";
            receipt += "================================\n";
            receipt += $"TOTAL:     Rp {payment.OrderTotal:N0}\n";

            if (payment.PaymentMethod == "Cash")
            {
                receipt += $"Paid:      Rp {payment.AmountPaid:N0}\n";
                receipt += $"Change:    Rp {payment.ChangeAmount:N0}\n";
            }

            receipt += "================================\n";
            receipt += "\n   Thank you for your order!\n";
            receipt += "================================\n";

            return receipt;
        }

        private async void OnPreviewReceipt()
        {
            var tempPayment = new PaymentResponse
            {
                OrderNumber = OrderNumber,
                PaymentMethod = IsCashSelected ? "Cash" : "QRIS",
                OrderTotal = Total,
                TaxAmount = Tax,
                AmountPaid = IsCashSelected ? decimal.Parse(CashAmountPaid ?? "0") : Total,
                ChangeAmount = IsCashSelected ? Change : 0,
                TransactionDate = DateTime.Now
            };

            await PrintReceiptAsync(tempPayment);
        }

        private async void OnCheckout()
        {
            if (!CartItems.Any())
            {
                await Application.Current!.MainPage!.DisplayAlert("Empty Cart",
                    "Please add items to cart first", "OK");
                return;
            }

            var navigationParameter = new Dictionary<string, object>
    {
        { "cartItems", CartItems.ToList() }
    };

            await Shell.Current.GoToAsync("checkout", navigationParameter);
        }

        private async void OnCancel()
        {
            bool confirm = await Application.Current!.MainPage!.DisplayAlert(
                "Cancel Checkout?",
                "Do you want to cancel this order?",
                "Yes, Cancel",
                "No");

            if (confirm)
            {
                try
                {
                    // Cancel the order
                    if (_orderId > 0)
                    {
                        await _apiService.CancelOrderAsync(_orderId);
                    }
                }
                catch
                {
                    // Ignore cancellation errors
                }

                await Shell.Current.GoToAsync("//main");
            }
        }

        private async void OnBackToCart()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}