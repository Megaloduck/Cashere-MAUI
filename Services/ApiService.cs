using Cashere.Models;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json; 
using System.Threading.Tasks;

namespace Cashere.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7102/api";
        private string _authToken;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService()
        {
            var handler = new HttpClientHandler();
            // For development - ignore SSL certificate validation
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        // ============ AUTHENTICATION ============
        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            try
            {
                var loginRequest = new LoginRequest
                {
                    Username = username,
                    Password = password
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/auth/login",
                    loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadAsAsync<LoginResponse>();
                    _authToken = loginResponse.Token;

                    // Save token for future requests
                    await SecureStorage.Default.SetAsync("auth_token", _authToken);
                    await SecureStorage.Default.SetAsync("user_id", loginResponse.Id.ToString());
                    await SecureStorage.Default.SetAsync("username", loginResponse.Username);
                    await SecureStorage.Default.SetAsync("role", loginResponse.Role);

                    return loginResponse;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Login failed: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Login error: {ex.Message}", ex);
            }
        }


        public async Task<bool> LogoutAsync()
        {
            try
            {
                SecureStorage.Default.Remove("auth_token");
                SecureStorage.Default.Remove("user_id");
                SecureStorage.Default.Remove("username");
                SecureStorage.Default.Remove("role");
                _authToken = null;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task LoadStoredTokenAsync()
        {
            try
            {
                _authToken = await SecureStorage.Default.GetAsync("auth_token");
            }
            catch
            {
                _authToken = null;
            }
        }

        public async Task<bool> ValidateTokenAsync()
        {
            try
            {
                await LoadStoredTokenAsync();

                if (string.IsNullOrEmpty(_authToken))
                    return false;

                SetAuthorizationHeader();
                var response = await _httpClient.PostAsync($"{_baseUrl}/auth/validate", null);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ============ MENU ============
        public async Task<List<MenuCategoryResponse>> GetMenuCategoriesAsync()
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var response = await _httpClient.GetAsync($"{_baseUrl}/menu/categories");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<List<MenuCategoryResponse>>();
                }

                throw new Exception($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load menu: {ex.Message}", ex);
            }
        }

        public async Task<List<MenuItemResponse>> GetMenuItemsByCategoryAsync(int categoryId)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/menu/category/{categoryId}/items");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<List<MenuItemResponse>>();
                }

                throw new Exception($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load items: {ex.Message}", ex);
            }
        }

        public async Task<MenuItemResponse> GetMenuItemAsync(int itemId)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var response = await _httpClient.GetAsync($"{_baseUrl}/menu/item/{itemId}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<MenuItemResponse>();
                }

                throw new Exception($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load item: {ex.Message}", ex);
            }
        }

        public async Task<TaxSettingsResponse> GetTaxSettingsAsync()
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var response = await _httpClient.GetAsync($"{_baseUrl}/menu/tax-settings");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<TaxSettingsResponse>();
                }

                throw new Exception($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load tax settings: {ex.Message}", ex);
            }
        }

        // ============ ORDERS ============
        public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest orderRequest)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var json = JsonSerializer.Serialize(orderRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/order/create", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<OrderResponse>();
                }

                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error creating order: {response.StatusCode} - {error}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create order: {ex.Message}", ex);
            }
        }

        public async Task<OrderResponse> GetOrderAsync(int orderId)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var response = await _httpClient.GetAsync($"{_baseUrl}/order/{orderId}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<OrderResponse>();
                }

                throw new Exception($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get order: {ex.Message}", ex);
            }
        }

        public async Task<OrderResponse> UpdateOrderAsync(int orderId, CreateOrderRequest orderRequest)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var json = JsonSerializer.Serialize(orderRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_baseUrl}/order/{orderId}", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<OrderResponse>();
                }

                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error updating order: {error}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update order: {ex.Message}", ex);
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var response = await _httpClient.DeleteAsync($"{_baseUrl}/order/{orderId}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to cancel order: {ex.Message}", ex);
            }
        }

        // ============ PAYMENTS ============
        public async Task<PaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest paymentRequest)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var json = JsonSerializer.Serialize(paymentRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/payment/process", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<PaymentResponse>();
                }

                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Payment failed: {error}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to process payment: {ex.Message}", ex);
            }
        }

        public async Task<PaymentResponse> GetPaymentAsync(int transactionId)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var response = await _httpClient.GetAsync($"{_baseUrl}/payment/{transactionId}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<PaymentResponse>();
                }

                throw new Exception($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get payment: {ex.Message}", ex);
            }
        }

        public async Task<List<TransactionDetailResponse>> GetDailyTransactionsAsync(DateTime date)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var dateString = date.ToString("yyyy-MM-dd");
                var response = await _httpClient.GetAsync($"{_baseUrl}/payment/daily/{dateString}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<List<TransactionDetailResponse>>();
                }

                throw new Exception($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get daily transactions: {ex.Message}", ex);
            }
        }

        // ============ REPORTS ============
        public async Task<DailySummaryResponse> GetDailySummaryAsync(DateTime date)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var dateString = date.ToString("yyyy-MM-dd");
                var response = await _httpClient.GetAsync($"{_baseUrl}/report/daily/{dateString}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<DailySummaryResponse>();
                }

                throw new Exception($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get daily summary: {ex.Message}", ex);
            }
        }

        // ============ HELPER METHODS ============
        private void SetAuthorizationHeader()
        {
            if (!string.IsNullOrEmpty(_authToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
            }
        }

        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(_authToken);
        }

        public async Task<string> GetStoredUsernameAsync()
        {
            return await SecureStorage.Default.GetAsync("username");
        }

        public async Task<string> GetStoredUserRoleAsync()
        {
            return await SecureStorage.Default.GetAsync("role");
        }

        // ============ ADMIN - CATEGORIES ============
        public async Task<MenuCategoryResponse> CreateCategoryAsync(string name, string description, int displayOrder)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var request = new { Name = name, Description = description, DisplayOrder = displayOrder };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/admin/MenuManagement/category", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<MenuCategoryResponse>();
                }

                throw new Exception($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create category: {ex.Message}", ex);
            }
        }

        public async Task<MenuCategoryResponse> UpdateCategoryAsync(int id, string name, string description, int displayOrder)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var request = new { Name = name, Description = description, DisplayOrder = displayOrder, IsActive = true };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_baseUrl}/admin/MenuManagement/category/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<MenuCategoryResponse>();
                }

                throw new Exception($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update category: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var response = await _httpClient.DeleteAsync($"{_baseUrl}/admin/MenuManagement/category/{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete category: {ex.Message}", ex);
            }
        }

        // ============ ADMIN - MENU ITEMS ============
        public async Task<MenuItemResponse> CreateMenuItemAsync( int categoryId, string name, string description, decimal price, bool isTaxable, decimal? customTaxRate, int displayOrder)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var request = new
                {
                    CategoryId = categoryId,
                    Name = name,
                    Description = description,
                    Price = price,
                    IsTaxable = isTaxable,
                    CustomTaxRate = customTaxRate,
                    DisplayOrder = displayOrder
                };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/admin/MenuManagement/item", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<MenuItemResponse>();
                }

                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {error}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create menu item: {ex.Message}", ex);
            }
        }

        public async Task<MenuItemResponse> UpdateMenuItemAsync( int id, int categoryId, string name, string description, decimal price, bool isTaxable, decimal? customTaxRate, bool isActive, int displayOrder)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var request = new
                {
                    CategoryId = categoryId,
                    Name = name,
                    Description = description,
                    Price = price,
                    IsTaxable = isTaxable,
                    CustomTaxRate = customTaxRate,
                    IsActive = isActive,
                    DisplayOrder = displayOrder
                };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_baseUrl}/admin/MenuManagement/item/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<MenuItemResponse>();
                }

                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {error}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update menu item: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteMenuItemAsync(int id)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var response = await _httpClient.DeleteAsync($"{_baseUrl}/admin/MenuManagement/item/{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete menu item: {ex.Message}", ex);
            }
        }

        // ============ ADMIN - TAX SETTINGS ============
        public async Task<TaxSettingsResponse> UpdateTaxSettingsAsync(string taxName, decimal taxRate, bool isEnabled)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var request = new { TaxName = taxName, DefaultTaxRate = taxRate, IsEnabled = isEnabled };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_baseUrl}/admin/MenuManagement/tax-settings", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<TaxSettingsResponse>();
                }

                throw new Exception($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update tax settings: {ex.Message}", ex);
            }
        }

        // ============ ADMIN - USERS ============
        public async Task<List<UserResponse>> GetAllUsersAsync()
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var response = await _httpClient.GetAsync($"{_baseUrl}/admin/UserManagement/users");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<List<UserResponse>>();
                }

                throw new Exception($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load users: {ex.Message}", ex);
            }
        }

        public async Task<UserResponse> CreateUserAsync(string username, string email, string password, string role)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var request = new { Username = username, Email = email, Password = password, Role = role };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/admin/UserManagement/users", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<UserResponse>();
                }

                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {error}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create user: {ex.Message}", ex);
            }
        }

        public async Task<UserResponse> UpdateUserAsync(int id, string username, string email, string role, bool isActive)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var request = new { Username = username, Email = email, Role = role, IsActive = isActive };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_baseUrl}/admin/UserManagement/users/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<UserResponse>();
                }

                throw new Exception($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update user: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var response = await _httpClient.DeleteAsync($"{_baseUrl}/admin/UserManagement/users/{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete user: {ex.Message}", ex);
            }
        }

        public async Task<bool> ResetUserPasswordAsync(int id, string newPassword)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var request = new { NewPassword = newPassword };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/admin/UserManagement/users/{id}/reset-password", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to reset password: {ex.Message}", ex);
            }
        }

        // ============ DASHBOARD ============
        public async Task<DailySummaryResponse> GetTodaySummaryAsync()
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var response = await _httpClient.GetAsync($"{_baseUrl}/Dashboard/summary/today");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<DailySummaryResponse>();
                }

                throw new Exception($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get today's summary: {ex.Message}", ex);
            }
        }

        public async Task<List<TransactionDetailResponse>> GetRecentTransactionsAsync(int count)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var response = await _httpClient.GetAsync($"{_baseUrl}/Dashboard/transactions/recent?count={count}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<List<TransactionDetailResponse>>();
                }

                throw new Exception($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get recent transactions: {ex.Message}", ex);
            }
        }

        public async Task<List<DailySummaryResponse>> GetDateRangeSummaryAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                await LoadStoredTokenAsync();
                SetAuthorizationHeader();

                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/Dashboard/summary/date-range?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<List<DailySummaryResponse>>();
                }

                throw new Exception($"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get date range summary: {ex.Message}", ex);
            }
        }
        public async Task<List<TransactionModel>> GetTransactionsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var url = $"{_baseUrl}/transactions?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                System.Diagnostics.Debug.WriteLine($"📡 GET {url}");
                var response = await _httpClient.GetAsync(url);

                var content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ GetTransactions failed: {response.StatusCode} - {content}");
                    return new List<TransactionModel>();
                }

                return JsonSerializer.Deserialize<List<TransactionModel>>(content, _jsonOptions) ?? new List<TransactionModel>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTransactions error: {ex}");
                return new List<TransactionModel>();
            }
        }

        public async Task<List<TopSellingItemResponse>> GetTopSellingItemsAsync(DateTime startDate, DateTime endDate, int count = 10)
        {
            try
            {
                var url = $"{_baseUrl}/report/top-items?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&count={count}";
                System.Diagnostics.Debug.WriteLine($"📡 GET {url}");

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ GetTopSellingItems failed: {response.StatusCode} - {content}");
                    return new List<TopSellingItemResponse>();
                }

                return JsonSerializer.Deserialize<List<TopSellingItemResponse>>(content, _jsonOptions)
                       ?? new List<TopSellingItemResponse>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTopSellingItems error: {ex.Message}");
                return new List<TopSellingItemResponse>();
            }
        }
    }

    // ============================================
    // Extension methods for HttpContent
    // ============================================
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            var json = await content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }

    // ============================================
    // Additional Response Models
    // ============================================
    public class ProcessPaymentRequest
    {
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; }
        public decimal AmountPaid { get; set; }
    }

    public class PaymentResponse
    {
        public int TransactionId { get; set; }
        public string OrderNumber { get; set; }
        public string PaymentMethod { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal ChangeAmount { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public string Status { get; set; }
        public string QRCodeData { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    public class TransactionDetailResponse
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string PaymentMethod { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public string Status { get; set; }
        public DateTime TransactionDate { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class DailySummaryResponse
    {
        public DateTime SummaryDate { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalTax { get; set; }
        public decimal CashCollected { get; set; }
        public decimal QRISCollected { get; set; }
        public int TotalItemsSold { get; set; }
    }

    public class UserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}