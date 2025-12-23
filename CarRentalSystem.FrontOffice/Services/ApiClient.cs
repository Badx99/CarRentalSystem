using CarRentalSystem.FrontOffice.Models.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CarRentalSystem.FrontOffice.Models.ViewModels;
using CarRentalSystem.Application.Features.Vehicles.Queries.GetVehicleById;

namespace CarRentalSystem.FrontOffice.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _baseUrl;
        private string? _authToken;

        public ApiClient(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _config = config;
            _baseUrl = _config["ApiSettings:BaseUrl"] ?? "http://localhost:5023/api/";
            
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        #region Token Management

        public void SetAuthToken(string token)
        {
            _authToken = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearAuthToken()
        {
            _authToken = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public bool HasToken => !string.IsNullOrEmpty(_authToken);

        #endregion

        #region Authentication (Public endpoints)

        public async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            try
            {
                var request = new LoginRequest { Email = email, Password = password };
                var response = await PostAsync<LoginResponse>("auth/login", request);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiClient] Login error: {ex.Message}");
                return null;
            }
        }

        public async Task<RegisterResponse?> RegisterCustomerAsync(RegisterCustomerRequest request)
        {
            try
            {
                return await PostAsync<RegisterResponse>("customer/register", request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiClient] Register error: {ex.Message}");
                return null;
            }
        }

        public async Task<CurrentUserResponse?> GetCurrentUserAsync()
        {
            try
            {
                return await GetAsync<CurrentUserResponse>("auth/me");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiClient] GetCurrentUser error: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Vehicles (Public endpoints)

        public async Task<List<VehicleDto>> GetAllVehiclesAsync()
        {
            try
            {
                var response = await GetAsync<VehiclesPagedResponse>("vehicles?pageSize=1000");
                return response?.Vehicles ?? new List<VehicleDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiClient] GetAllVehicles error: {ex.Message}");
                return new List<VehicleDto>();
            }
        }

        public async Task<GetVehicleByIdResponse?> GetVehicleByIdAsync(Guid id)
        {
            try
            {
                return await GetAsync<GetVehicleByIdResponse>($"vehicles/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiClient] GetVehicleById error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<VehicleDto>> GetAvailableVehiclesAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var start = startDate.ToString("yyyy-MM-dd");
                var end = endDate.ToString("yyyy-MM-dd");
                var response = await GetAsync<VehiclesPagedResponse>($"vehicles/available?startDate={start}&endDate={end}&pageSize=1000");
                return response?.Vehicles ?? new List<VehicleDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiClient] GetAvailableVehicles error: {ex.Message}");
                return new List<VehicleDto>();
            }
        }

        public async Task<List<VehicleTypeDto>> GetVehicleTypesAsync()
        {
            try
            {
                var response = await GetAsync<VehicleTypesPagedResponse>("vehicletypes?pageSize=100");
                return response?.VehicleTypes ?? new List<VehicleTypeDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiClient] GetVehicleTypes error: {ex.Message}");
                return new List<VehicleTypeDto>();
            }
        }

        public async Task<PagedResult<VehicleDto>> SearchVehiclesAsync(SearchVehiclesQuery query)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"page={query.Page}",
                    $"pageSize={query.PageSize}"
                };

                if (!string.IsNullOrEmpty(query.SearchTerm))
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(query.SearchTerm)}");
                if (query.VehicleTypeId.HasValue)
                    queryParams.Add($"vehicleTypeId={query.VehicleTypeId}");
                if (query.MinPrice.HasValue)
                    queryParams.Add($"minPrice={query.MinPrice}");
                if (query.MaxPrice.HasValue)
                    queryParams.Add($"maxPrice={query.MaxPrice}");
                if (!string.IsNullOrEmpty(query.Status))
                    queryParams.Add($"status={query.Status}");

                var url = $"vehicles/search?{string.Join("&", queryParams)}";
                var response = await GetAsync<PagedResult<VehicleDto>>(url);

                return response ?? new PagedResult<VehicleDto>
                {
                    Items = new List<VehicleDto>(),
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalCount = 0
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiClient] SearchVehicles error: {ex.Message}");
                return new PagedResult<VehicleDto>();
            }
        }

        #endregion

        #region Reservations (Authenticated)

        public async Task<List<ReservationDto>> GetMyReservationsAsync()
        {
            try
            {
                var response = await GetAsync<ReservationsPagedResponse>("reservations/my-reservations?pageSize=1000");
                return response?.Reservations ?? new List<ReservationDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiClient] GetMyReservations error: {ex.Message}");
                return new List<ReservationDto>();
            }
        }

        public async Task<ReservationDetailDto?> GetReservationByIdAsync(Guid id)
        {
            try
            {
                return await GetAsync<ReservationDetailDto>($"reservations/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiClient] GetReservationById error: {ex.Message}");
                return null;
            }
        }

        public async Task<CreateReservationResponse?> CreateReservationAsync(CreateReservationRequest request)
        {
            try
            {
                return await PostAsync<CreateReservationResponse>("reservations", request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiClient] CreateReservation error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CancelReservationAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"reservations/{id}/cancel", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiClient] CancelReservation error: {ex.Message}");
                return false;
            }
        }

        public async Task<byte[]?> DownloadReceiptAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"reservations/{id}/receipt");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiClient] DownloadReceipt error: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Helper Methods

        private async Task<T?> GetAsync<T>(string endpoint) where T : class
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                Console.WriteLine($"[ApiClient] GET {endpoint} returned {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiClient] GET {endpoint} error: {ex.Message}");
                return null;
            }
        }

        private async Task<T?> PostAsync<T>(string endpoint, object data) where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                Console.WriteLine($"[ApiClient] POST {endpoint} returned {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiClient] POST {endpoint} error: {ex.Message}");
                return null;
            }
        }

        #endregion
    }

    #region Response Wrappers

    public class VehiclesPagedResponse
    {
        public List<VehicleDto> Vehicles { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class VehicleTypesPagedResponse
    {
        public List<VehicleTypeDto> VehicleTypes { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ReservationsPagedResponse
    {
        public List<ReservationDto> Reservations { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    #endregion
}