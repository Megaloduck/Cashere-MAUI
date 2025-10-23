using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microcharts;
using SkiaSharp;
using Cashere.Models;
using Cashere.Services;

namespace Cashere.PageModels
{
    public class EnhancedDashboardPageModel : BasePageModel
    {
        private readonly ApiService _apiService;

        // Summary properties
        private decimal _totalRevenue;
        private int _totalTransactions;
        private decimal _totalCash;
        private decimal _totalQRIS;
        private decimal _totalTax;
        private int _itemsSold;
        private double _cashPercentage;
        private double _qrisPercentage;

        // Period selection
        private bool _isTodaySelected = true;
        private bool _isWeekSelected;
        private string _selectedPeriod = "Today";

        // Charts
        private Chart _paymentMethodChart;
        private Chart _salesTrendChart;
        private Chart _hourlySalesChart;

        // Collections
        private ObservableCollection<TopSellingItemModel> _topSellingItems;
        private ObservableCollection<TransactionDetailModel> _recentTransactions;

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set { _totalRevenue = value; OnPropertyChanged(); }
        }

        public int TotalTransactions
        {
            get => _totalTransactions;
            set { _totalTransactions = value; OnPropertyChanged(); }
        }

        public decimal TotalCash
        {
            get => _totalCash;
            set { _totalCash = value; OnPropertyChanged(); }
        }

        public decimal TotalQRIS
        {
            get => _totalQRIS;
            set { _totalQRIS = value; OnPropertyChanged(); }
        }

        public decimal TotalTax
        {
            get => _totalTax;
            set { _totalTax = value; OnPropertyChanged(); }
        }

        public int ItemsSold
        {
            get => _itemsSold;
            set { _itemsSold = value; OnPropertyChanged(); }
        }

        public double CashPercentage
        {
            get => _cashPercentage;
            set { _cashPercentage = value; OnPropertyChanged(); }
        }

        public double QRISPercentage
        {
            get => _qrisPercentage;
            set { _qrisPercentage = value; OnPropertyChanged(); }
        }

        public bool IsTodaySelected
        {
            get => _isTodaySelected;
            set { _isTodaySelected = value; OnPropertyChanged(); }
        }

        public bool IsWeekSelected
        {
            get => _isWeekSelected;
            set { _isWeekSelected = value; OnPropertyChanged(); }
        }

        public Chart PaymentMethodChart
        {
            get => _paymentMethodChart;
            set { _paymentMethodChart = value; OnPropertyChanged(); }
        }

        public Chart SalesTrendChart
        {
            get => _salesTrendChart;
            set { _salesTrendChart = value; OnPropertyChanged(); }
        }

        public Chart HourlySalesChart
        {
            get => _hourlySalesChart;
            set { _hourlySalesChart = value; OnPropertyChanged(); }
        }

        public ObservableCollection<TopSellingItemModel> TopSellingItems
        {
            get => _topSellingItems;
            set { _topSellingItems = value; OnPropertyChanged(); }
        }

        public ObservableCollection<TransactionDetailModel> RecentTransactions
        {
            get => _recentTransactions;
            set { _recentTransactions = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand SelectPeriodCommand { get; }
        public ICommand ViewAllTransactionsCommand { get; }
        public ICommand ExportToExcelCommand { get; }
        public ICommand PrintReportCommand { get; }

        public EnhancedDashboardPageModel()
        {
            _apiService = new ApiService();
            TopSellingItems = new ObservableCollection<TopSellingItemModel>();
            RecentTransactions = new ObservableCollection<TransactionDetailModel>();

            RefreshCommand = new Command(async () => await InitializeAsync());
            BackCommand = new Command(async () => await Shell.Current.GoToAsync("//main"));
            SelectPeriodCommand = new Command<string>(OnSelectPeriod);
            ViewAllTransactionsCommand = new Command(OnViewAllTransactions);
            ExportToExcelCommand = new Command(OnExportToExcel);
            PrintReportCommand = new Command(OnPrintReport);
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;

                await LoadDashboardDataAsync();
                await LoadChartsAsync();
                await LoadTopSellingItemsAsync();
                await LoadRecentTransactionsAsync();
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load dashboard: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnSelectPeriod(string period)
        {
            _selectedPeriod = period;
            IsTodaySelected = period == "Today";
            IsWeekSelected = period == "Week";

            MainThread.BeginInvokeOnMainThread(async () => await InitializeAsync());
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                if (_selectedPeriod == "Today")
                {
                    var summary = await _apiService.GetTodaySummaryAsync();

                    TotalRevenue = summary.TotalRevenue;
                    TotalTransactions = summary.TotalTransactions;
                    TotalCash = summary.CashCollected;
                    TotalQRIS = summary.QRISCollected;
                    TotalTax = summary.TotalTax;
                    ItemsSold = summary.TotalItemsSold;
                }
                else // Week
                {
                    var endDate = DateTime.Now.Date;
                    var startDate = endDate.AddDays(-6);
                    var summaries = await _apiService.GetDateRangeSummaryAsync(startDate, endDate);

                    TotalRevenue = summaries.Sum(s => s.TotalRevenue);
                    TotalTransactions = summaries.Sum(s => s.TotalTransactions);
                    TotalCash = summaries.Sum(s => s.CashCollected);
                    TotalQRIS = summaries.Sum(s => s.QRISCollected);
                    TotalTax = summaries.Sum(s => s.TotalTax);
                    ItemsSold = summaries.Sum(s => s.TotalItemsSold);
                }

                // Calculate percentages
                if (TotalRevenue > 0)
                {
                    CashPercentage = (double)(TotalCash / TotalRevenue * 100);
                    QRISPercentage = (double)(TotalQRIS / TotalRevenue * 100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
            }
        }

        private async Task LoadChartsAsync()
        {
            try
            {
                // Payment Method Pie Chart
                var paymentEntries = new List<ChartEntry>
                {
                    new ChartEntry((float)TotalCash)
                    {
                        Label = "Cash",
                        ValueLabel = $"Rp {TotalCash:N0}",
                        Color = SKColor.Parse("#3498DB")
                    },
                    new ChartEntry((float)TotalQRIS)
                    {
                        Label = "QRIS",
                        ValueLabel = $"Rp {TotalQRIS:N0}",
                        Color = SKColor.Parse("#9B59B6")
                    }
                };

                PaymentMethodChart = new DonutChart
                {
                    Entries = paymentEntries,
                    LabelTextSize = 30,
                    LabelMode = LabelMode.RightOnly,
                    GraphPosition = GraphPosition.Center
                };

                // Sales Trend Line Chart (Last 7 days)
                var endDate = DateTime.Now.Date;
                var startDate = endDate.AddDays(-6);
                var summaries = await _apiService.GetDateRangeSummaryAsync(startDate, endDate);

                var trendEntries = new List<ChartEntry>();
                for (int i = 0; i < 7; i++)
                {
                    var date = startDate.AddDays(i);
                    var summary = summaries.FirstOrDefault(s => s.SummaryDate.Date == date.Date);
                    var revenue = summary?.TotalRevenue ?? 0;

                    trendEntries.Add(new ChartEntry((float)revenue)
                    {
                        Label = date.ToString("dd/MM"),
                        ValueLabel = $"Rp {revenue:N0}",
                        Color = SKColor.Parse("#27AE60")
                    });
                }

                SalesTrendChart = new LineChart
                {
                    Entries = trendEntries,
                    LabelTextSize = 30,
                    LineMode = LineMode.Straight,
                    PointMode = PointMode.Circle,
                    PointSize = 15
                };

                // Hourly Sales Bar Chart
                await LoadHourlySalesChartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading charts: {ex.Message}");
            }
        }

        private async Task LoadHourlySalesChartAsync()
        {
            try
            {
                // Simulate hourly sales data (in production, get from API)
                var hourlyData = new Dictionary<int, decimal>();
                var transactions = await _apiService.GetDailyTransactionsAsync(DateTime.Now.Date);

                foreach (var transaction in transactions)
                {
                    var hour = transaction.TransactionDate.Hour;
                    if (!hourlyData.ContainsKey(hour))
                        hourlyData[hour] = 0;

                    hourlyData[hour] += transaction.OrderTotal;
                }

                var hourlyEntries = new List<ChartEntry>();
                for (int hour = 6; hour <= 22; hour++) // 6 AM to 10 PM
                {
                    var amount = hourlyData.ContainsKey(hour) ? hourlyData[hour] : 0;
                    hourlyEntries.Add(new ChartEntry((float)amount)
                    {
                        Label = $"{hour:D2}:00",
                        ValueLabel = amount > 0 ? $"Rp {amount / 1000:N0}K" : "",
                        Color = SKColor.Parse("#E67E22")
                    });
                }

                HourlySalesChart = new BarChart
                {
                    Entries = hourlyEntries,
                    LabelTextSize = 25,
                    ValueLabelOrientation = Orientation.Horizontal
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading hourly chart: {ex.Message}");
            }
        }

        private async Task LoadTopSellingItemsAsync()
        {
            try
            {
                // Get transactions and calculate top items
                var transactions = _selectedPeriod == "Today"
                    ? await _apiService.GetDailyTransactionsAsync(DateTime.Now.Date)
                    : await GetWeekTransactionsAsync();

                var itemSales = new Dictionary<string, (int quantity, decimal revenue, string category)>();

                foreach (var transaction in transactions)
                {
                    foreach (var item in transaction.Items)
                    {
                        if (!itemSales.ContainsKey(item.ItemName))
                            itemSales[item.ItemName] = (0, 0, "Unknown");

                        var current = itemSales[item.ItemName];
                        itemSales[item.ItemName] = (
                            current.quantity + item.Quantity,
                            current.revenue + item.TotalAmount,
                            current.category
                        );
                    }
                }

                TopSellingItems.Clear();
                var topItems = itemSales
                    .OrderByDescending(x => x.Value.quantity)
                    .Take(5)
                    .Select((item, index) => new TopSellingItemModel
                    {
                        Rank = index + 1,
                        ItemName = item.Key,
                        Category = item.Value.category,
                        QuantitySold = item.Value.quantity,
                        Revenue = item.Value.revenue
                    });

                foreach (var item in topItems)
                {
                    TopSellingItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading top items: {ex.Message}");
            }
        }

        private async Task LoadRecentTransactionsAsync()
        {
            try
            {
                var transactions = await _apiService.GetRecentTransactionsAsync(10);

                RecentTransactions.Clear();
                foreach (var t in transactions)
                {
                    RecentTransactions.Add(new TransactionDetailModel
                    {
                        OrderNumber = t.OrderNumber,
                        PaymentMethod = t.PaymentMethod,
                        OrderTotal = t.OrderTotal,
                        TaxAmount = t.TaxAmount,
                        TransactionDate = t.TransactionDate,
                        Status = t.Status,
                        ItemCount = t.Items?.Count ?? 0
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading transactions: {ex.Message}");
            }
        }

        private async Task<List<TransactionDetailResponse>> GetWeekTransactionsAsync()
        {
            var allTransactions = new List<TransactionDetailResponse>();
            var endDate = DateTime.Now.Date;
            var startDate = endDate.AddDays(-6);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                try
                {
                    var transactions = await _apiService.GetDailyTransactionsAsync(date);
                    allTransactions.AddRange(transactions);
                }
                catch
                {
                    // Skip days with no data
                }
            }

            return allTransactions;
        }

        private async void OnViewAllTransactions()
        {
            await Application.Current!.MainPage!.DisplayAlert("All Transactions", "View all transactions page (implement full transaction history)", "OK");
        }

        private async void OnExportToExcel()
        {
            try
            {
                IsLoading = true;

                // Simulate export
                await Task.Delay(1000);

                await Application.Current!.MainPage!.DisplayAlert("Export",
                    $"Exported {TotalTransactions} transactions to Excel!\n\nFile: CafePOS_Report_{DateTime.Now:yyyyMMdd}.xlsx",
                    "OK");
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

        private async void OnPrintReport()
        {
            try
            {
                IsLoading = true;

                // Simulate print
                await Task.Delay(1000);

                await Application.Current!.MainPage!.DisplayAlert("Print",
                    "Report sent to printer!\n\nPrinting sales summary and transaction details.",
                    "OK");
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
    }

    // Supporting Models
    public class TopSellingItemModel
    {
        public int Rank { get; set; }
        public string ItemName { get; set; }
        public string Category { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TransactionDetailModel
    {
        public string OrderNumber { get; set; }
        public string PaymentMethod { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }
        public int ItemCount { get; set; }
    }
}