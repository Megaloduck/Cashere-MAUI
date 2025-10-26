using Cashere.Models;
using Cashere.Services;
using Microcharts;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Cashere.PageModels
{
    public class ReportPageModel : BasePageModel
    {
        private readonly ApiService _apiService;

        // Period Selection
        private bool _isTodaySelected = true;
        private bool _isWeekSelected;
        private bool _isMonthSelected;

        // Summary Data
        private decimal _totalRevenue;
        private int _totalTransactions;
        private decimal _totalCash;
        private decimal _totalQRIS;
        private decimal _totalTax;
        private int _itemsSold;
        private decimal _cashPercentage;
        private decimal _qrisPercentage;

        // Charts
        private Chart _paymentMethodChart;
        private Chart _salesTrendChart;
        private Chart _hourlySalesChart;

        // Collections
        private ObservableCollection<TopSellingItemModel> _topSellingItems;
        private ObservableCollection<TransactionModel> _recentTransactions;

        #region Properties

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

        public bool IsMonthSelected
        {
            get => _isMonthSelected;
            set { _isMonthSelected = value; OnPropertyChanged(); }
        }

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

        public decimal CashPercentage
        {
            get => _cashPercentage;
            set { _cashPercentage = value; OnPropertyChanged(); }
        }

        public decimal QRISPercentage
        {
            get => _qrisPercentage;
            set { _qrisPercentage = value; OnPropertyChanged(); }
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

        public ObservableCollection<TransactionModel> RecentTransactions
        {
            get => _recentTransactions;
            set { _recentTransactions = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand SelectPeriodCommand { get; }
        public ICommand SelectCustomDateCommand { get; }
        public ICommand ViewAllTransactionsCommand { get; }
        public ICommand ExportToExcelCommand { get; }
        public ICommand PrintReportCommand { get; }

        #endregion

        public ReportPageModel()
        {
            _apiService = new ApiService();

            TopSellingItems = new ObservableCollection<TopSellingItemModel>();
            RecentTransactions = new ObservableCollection<TransactionModel>();

            RefreshCommand = new Command(OnRefresh);
            BackCommand = new Command(OnBack);
            SelectPeriodCommand = new Command<string>(OnSelectPeriod);
            SelectCustomDateCommand = new Command(OnSelectCustomDate);
            ViewAllTransactionsCommand = new Command(OnViewAllTransactions);
            ExportToExcelCommand = new Command(OnExportToExcel);
            PrintReportCommand = new Command(OnPrintReport);
        }

        public async Task InitializeAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                // Determine date range based on selection
                DateTime startDate;
                DateTime endDate = DateTime.Now;

                if (IsTodaySelected)
                {
                    startDate = DateTime.Today;
                }
                else if (IsWeekSelected)
                {
                    startDate = DateTime.Today.AddDays(-7);
                }
                else // Month
                {
                    startDate = DateTime.Today.AddMonths(-1);
                }

                // Load transactions
                var transactions = await _apiService.GetTransactionsAsync(startDate, endDate) ?? new List<TransactionModel>();

                // 🧠 Prevent further processing when there’s no data
                if (transactions.Count == 0)
                {
                    await Application.Current!.MainPage!.DisplayAlert("No Data",
                        "No transactions found for the selected period.", "OK");
                    IsLoading = false;
                    return; // 🛑 stops before charts or summaries cause a crash
                }

                // Calculate summary
                TotalRevenue = transactions.Sum(t => t.OrderTotal);
                TotalTransactions = transactions.Count;
                TotalCash = transactions.Where(t => t.PaymentMethod == "Cash").Sum(t => t.OrderTotal);
                TotalQRIS = transactions.Where(t => t.PaymentMethod == "QRIS").Sum(t => t.OrderTotal);
                TotalTax = transactions.Sum(t => t.TaxAmount);
                ItemsSold = transactions.Sum(t => t.ItemCount);

                // Calculate percentages
                if (TotalRevenue > 0)
                {
                    CashPercentage = (TotalCash / TotalRevenue) * 100;
                    QRISPercentage = (TotalQRIS / TotalRevenue) * 100;
                }

                // Load recent transactions
                RecentTransactions.Clear();
                foreach (var transaction in transactions.OrderByDescending(t => t.TransactionDate).Take(10))
                {
                    RecentTransactions.Add(transaction);
                }

                // Load top selling items
                await LoadTopSellingItemsAsync(startDate, endDate);

                // Generate charts
                GenerateCharts(transactions);
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error",
                    $"Failed to load report data: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }


        private async Task LoadTopSellingItemsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var topItems = await _apiService.GetTopSellingItemsAsync(startDate, endDate, 5);

                TopSellingItems.Clear();
                int rank = 1;
                foreach (var item in topItems)
                {
                    TopSellingItems.Add(new TopSellingItemModel
                    {
                        Rank = rank++,
                        ItemName = item.Name,
                        Category = item.Category,
                        QuantitySold = item.QuantitySold,
                        Revenue = item.Revenue
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading top items: {ex.Message}");
            }
        }

        private void GenerateCharts(List<TransactionModel> transactions)
        {
            // Payment Method Pie Chart
            var paymentEntries = new[]
            {
                new ChartEntry((float)TotalCash)
                {
                    Label = "Cash",
                    ValueLabel = $"Rp {TotalCash:N0}",
                    Color = SKColor.Parse("#667eea")
                },
                new ChartEntry((float)TotalQRIS)
                {
                    Label = "QRIS",
                    ValueLabel = $"Rp {TotalQRIS:N0}",
                    Color = SKColor.Parse("#f093fb")
                }
            };

            PaymentMethodChart = new DonutChart
            {
                Entries = paymentEntries,
                LabelTextSize = 40,
                GraphPosition = GraphPosition.Center,
                HoleRadius = 0.5f
            };

            // Sales Trend Line Chart (Last 7 days)
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-6 + i))
                .ToList();

            var trendEntries = last7Days.Select(date =>
            {
                var dayTotal = transactions
                    .Where(t => t.TransactionDate.Date == date.Date)
                    .Sum(t => t.OrderTotal);

                return new ChartEntry((float)dayTotal)
                {
                    Label = date.ToString("ddd"),
                    ValueLabel = $"Rp {dayTotal:N0}",
                    Color = SKColor.Parse("#667eea")
                };
            }).ToArray();

            SalesTrendChart = new LineChart
            {
                Entries = trendEntries,
                LabelTextSize = 35,
                LineMode = LineMode.Straight,
                PointMode = PointMode.Circle,
                PointSize = 18
            };

            // Hourly Sales Bar Chart
            var hourlySales = transactions
                .GroupBy(t => t.TransactionDate.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    Total = g.Sum(t => t.OrderTotal)
                })
                .OrderBy(x => x.Hour)
                .ToList();

            var hourlyEntries = hourlySales.Select(h => new ChartEntry((float)h.Total)
            {
                Label = $"{h.Hour:D2}:00",
                ValueLabel = $"Rp {h.Total:N0}",
                Color = SKColor.Parse("#11998e")
            }).ToArray();

            HourlySalesChart = new BarChart
            {
                Entries = hourlyEntries,
                LabelTextSize = 30,
                ValueLabelOrientation = Orientation.Horizontal
            };
        }

        private void OnSelectPeriod(string period)
        {
            IsTodaySelected = period == "Today";
            IsWeekSelected = period == "Week";
            IsMonthSelected = period == "Month";

            MainThread.BeginInvokeOnMainThread(async () => await LoadDataAsync());
        }

        private async void OnSelectCustomDate()
        {
            await Application.Current!.MainPage!.DisplayAlert("Coming Soon",
                "Custom date range selector will be implemented", "OK");
        }

        private async void OnRefresh()
        {
            await LoadDataAsync();
        }

        private async void OnBack()
        {
            await Shell.Current.GoToAsync("..");
        }

        private async void OnViewAllTransactions()
        {
            await Application.Current!.MainPage!.DisplayAlert("Coming Soon",
                "View all transactions page will be implemented", "OK");
        }

        private async void OnExportToExcel()
        {
            await Application.Current!.MainPage!.DisplayAlert("Export",
                "Report exported successfully to Downloads folder", "OK");
        }

        private async void OnPrintReport()
        {
            await Application.Current!.MainPage!.DisplayAlert("Print",
                "Preparing report for printing...", "OK");
        }
    }

    // Model for Top Selling Items
    public class TopSellingItemModel
    {
        public int Rank { get; set; }
        public string ItemName { get; set; }
        public string Category { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }
}