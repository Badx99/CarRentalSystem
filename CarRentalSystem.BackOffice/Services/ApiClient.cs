using CarRentalSystem.BackOffice.Models.DTOs;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CarRentalSystem.BackOffice.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5023/api/";
        private const int TimeoutSeconds = 30;
        private const int MaxRetries = 2;

        public event Action? OnUnauthorized;

        public ApiClient()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(TimeoutSeconds)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearAuthToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        #region Helper Methods

        private async Task<T?> GetAsync<T>(string endpoint) where T : class
        {
            return await ExecuteWithRetry(async () =>
            {
                try
                {
                    var response = await _httpClient.GetAsync(endpoint);
                    return await HandleResponse<T>(response);
                }
                catch (Exception ex)
                {
                    LogError($"GET {endpoint}", ex);
                    return default;
                }
            });
        }

        private async Task<T?> PostAsync<T>(string endpoint, object? data = null) where T : class
        {
            return await ExecuteWithRetry(async () =>
            {
                try
                {
                    var content = data != null
                        ? new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
                        : null;

                    var response = await _httpClient.PostAsync(endpoint, content);
                    return await HandleResponse<T>(response);
                }
                catch (Exception ex)
                {
                    LogError($"POST {endpoint}", ex);
                    return default;
                }
            });
        }

        private async Task<bool> PostAsync(string endpoint, object? data = null)
        {
            return await ExecuteWithRetry(async () =>
            {
                try
                {
                    var content = data != null
                        ? new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
                        : null;

                    var response = await _httpClient.PostAsync(endpoint, content);
                    return await HandleBoolResponse(response);
                }
                catch (Exception ex)
                {
                    LogError($"POST {endpoint}", ex);
                    return false;
                }
            });
        }

        private async Task<bool> PutAsync(string endpoint, object data)
        {
            return await ExecuteWithRetry(async () =>
            {
                try
                {
                    var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PutAsync(endpoint, content);
                    return await HandleBoolResponse(response);
                }
                catch (Exception ex)
                {
                    LogError($"PUT {endpoint}", ex);
                    return false;
                }
            });
        }

        private async Task<bool> PatchAsync(string endpoint, object data)
        {
            return await ExecuteWithRetry(async () =>
            {
                try
                {
                    var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                    var method = new HttpMethod("PATCH");
                    var request = new HttpRequestMessage(method, endpoint) { Content = content };
                    var response = await _httpClient.SendAsync(request);
                    return await HandleBoolResponse(response);
                }
                catch (Exception ex)
                {
                    LogError($"PATCH {endpoint}", ex);
                    return false;
                }
            });
        }

        private async Task<bool> DeleteAsync(string endpoint)
        {
            return await ExecuteWithRetry(async () =>
            {
                try
                {
                    var response = await _httpClient.DeleteAsync(endpoint);
                    return await HandleBoolResponse(response);
                }
                catch (Exception ex)
                {
                    LogError($"DELETE {endpoint}", ex);
                    return false;
                }
            });
        }

        private async Task<byte[]?> GetFileAsync(string endpoint)
        {
            return await ExecuteWithRetry(async () =>
            {
                try
                {
                    var response = await _httpClient.GetAsync(endpoint);
                    
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        OnUnauthorized?.Invoke();
                        return null;
                    }

                    if (response.IsSuccessStatusCode)
                    {
                         return await response.Content.ReadAsByteArrayAsync();
                    }
                    
                    return null;
                }
                catch (Exception ex)
                {
                    LogError($"GET FILE {endpoint}", ex);
                    return null;
                }
            });
        }

        private async Task<T?> HandleResponse<T>(HttpResponseMessage response) where T : class
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                OnUnauthorized?.Invoke();
                throw new ApiException("Unauthorized access", response.StatusCode, string.Empty);
            }

            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[ApiClient Response] {content.Substring(0, Math.Min(500, content.Length))}...");
                return JsonConvert.DeserializeObject<T>(content);
            }

            LogError($"Response error: {response.StatusCode} - {content}", null);
            throw new ApiException($"API Error: {response.StatusCode}", response.StatusCode, content);
        }

        private async Task<bool> HandleBoolResponse(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                OnUnauthorized?.Invoke();
                throw new ApiException("Unauthorized access", response.StatusCode, string.Empty);
            }

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                LogError($"Response error: {response.StatusCode} - {content}", null);
                throw new ApiException($"API Error: {response.StatusCode}", response.StatusCode, content);
            }

            return response.IsSuccessStatusCode;
        }

        private static async Task<T?> ExecuteWithRetry<T>(Func<Task<T?>> action)
        {
            for (int i = 0; i <= MaxRetries; i++)
            {
                try
                {
                    return await action();
                }
                catch (TaskCanceledException) when (i < MaxRetries)
                {
                    Debug.WriteLine($"Request timeout, retry {i + 1}/{MaxRetries}");
                    await Task.Delay(1000 * (i + 1));
                }
                catch (HttpRequestException) when (i < MaxRetries)
                {
                    Debug.WriteLine($"Connection error, retry {i + 1}/{MaxRetries}");
                    await Task.Delay(1000 * (i + 1));
                }
            }
            return default;
        }

        private static void LogError(string context, Exception? ex)
        {
            var message = ex != null ? $"{context}: {ex.Message}" : context;
            Debug.WriteLine($"[ApiClient Error] {message}");
            Console.WriteLine($"[ApiClient Error] {message}");
        }

        #endregion

        #region Authentication

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            return await PostAsync<LoginResponse>("auth/login", request);
        }

        #endregion

        #region Dashboard

        public async Task<DashboardStatsDto?> GetDashboardStatsAsync()
        {
            return await GetAsync<DashboardStatsDto>("dashboard/stats");
        }

        #endregion

        #region Vehicles

        public async Task<List<VehicleDto>> GetVehiclesAsync()
        {
            // API returns paginated response, extract Vehicles list
            var response = await GetAsync<VehiclesPagedResponse>("vehicles?pageSize=1000");
            return response?.Vehicles ?? new List<VehicleDto>();
        }

        public async Task<VehicleDto?> GetVehicleByIdAsync(Guid id)
        {
            // Single vehicle response wraps in Vehicle property
            var response = await GetAsync<VehicleByIdResponse>($"vehicles/{id}");
            return response?.Vehicle;
        }

        public async Task<VehicleDto?> CreateVehicleAsync(CreateVehicleRequest request)
        {
            return await PostAsync<VehicleDto>("vehicles", request);
        }

        public async Task<bool> UpdateVehicleAsync(Guid id, UpdateVehicleRequest request)
        {
            return await PutAsync($"vehicles/{id}", request);
        }

        public async Task<bool> UpdateVehicleStatusAsync(Guid id, string status)
        {
            var request = new UpdateVehicleStatusCommand { Id = id, Status = status };
            return await PatchAsync($"vehicles/{id}/status", request);
        }

        public async Task<bool> DeleteVehicleAsync(Guid id)
        {
            return await DeleteAsync($"vehicles/{id}");
        }

        public async Task<byte[]?> ExportVehiclesAsync()
        {
            return await GetFileAsync("exports/vehicles");
        }

        #endregion

        #region Vehicle Types

        public async Task<List<VehicleTypeDto>> GetVehicleTypesAsync()
        {
            // API returns paginated response, extract VehicleTypes list
            var response = await GetAsync<VehicleTypesPagedResponse>("vehicletypes?pageSize=1000");
            return response?.VehicleTypes ?? new List<VehicleTypeDto>();
        }

        public async Task<VehicleTypeDto?> GetVehicleTypeByIdAsync(Guid id)
        {
            return await GetAsync<VehicleTypeDto>($"vehicletypes/{id}");
        }

        public async Task<VehicleTypeDto?> CreateVehicleTypeAsync(CreateVehicleTypeRequest request)
        {
            return await PostAsync<VehicleTypeDto>("vehicletypes", request);
        }

        public async Task<bool> UpdateVehicleTypeAsync(Guid id, UpdateVehicleTypeRequest request)
        {
            return await PutAsync($"vehicletypes/{id}", request);
        }

        public async Task<bool> DeleteVehicleTypeAsync(Guid id)
        {
            return await DeleteAsync($"vehicletypes/{id}");
        }

        #endregion

        #region Reservations

        public async Task<List<ReservationDto>> GetReservationsAsync()
        {
            // API returns paginated response, extract Reservations list
            var response = await GetAsync<ReservationsPagedResponse>("reservations?pageSize=1000");
            return response?.Reservations ?? new List<ReservationDto>();
        }

        public async Task<ReservationDto?> GetReservationByIdAsync(Guid id)
        {
            return await GetAsync<ReservationDto>($"reservations/{id}");
        }

        public async Task<GetReservationsByCustomerResponse?> GetReservationsByCustomerAsync(Guid customerId)
        {
            return await GetAsync<GetReservationsByCustomerResponse>($"reservations/customer/{customerId}?pageSize=1000");
        }

        public async Task<bool> ConfirmReservationAsync(Guid id)
        {
            return await PostAsync($"reservations/{id}/confirm");
        }

        public async Task<bool> StartRentalAsync(Guid id)
        {
            return await PostAsync($"reservations/{id}/start");
        }

        public async Task<bool> CompleteReservationAsync(Guid id, int finalMileage)
        {
            var request = new { reservationId = id, finalMileage };
            return await PostAsync($"reservations/{id}/complete", request);
        }

        public async Task<bool> CancelReservationAsync(Guid id)
        {
            return await PostAsync($"reservations/{id}/cancel");
        }

        public async Task<byte[]?> DownloadReceiptAsync(Guid id)
        {
            return await GetFileAsync($"reservations/{id}/receipt");
        }

        public async Task<byte[]?> ExportReservationsAsync()
        {
            return await GetFileAsync("exports/reservations");
        }

        #endregion

        #region Customers

        public async Task<List<CustomerDto>> GetCustomersAsync()
        {
            // API returns paginated response with Customers list
            var response = await GetAsync<CustomersPagedResponse>("customer?pageSize=1000");
            return response?.Customers ?? new List<CustomerDto>();
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id)
        {
            return await GetAsync<CustomerDto>($"customer/{id}");
        }

        #endregion

        #region Employees (Admin only)

        public async Task<List<EmployeeDto>> GetEmployeesAsync()
        {
            var response = await GetAsync<EmployeesPagedResponse>("employees?pageSize=1000");
            return response?.Employees ?? new List<EmployeeDto>();
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id)
        {
            return await GetAsync<EmployeeDto>($"employees/{id}");
        }

        public async Task<EmployeeDto?> CreateEmployeeAsync(CreateEmployeeRequest request)
        {
            return await PostAsync<EmployeeDto>("employees", request);
        }

        public async Task<bool> UpdateEmployeeAsync(Guid id, UpdateEmployeeRequest request)
        {
            return await PutAsync($"employees/{id}", request);
        }

        public async Task<bool> DeleteEmployeeAsync(Guid id)
        {
            return await DeleteAsync($"employees/{id}");
        }
        
        #endregion

        #region Payments

        public async Task<List<PaymentDto>> GetPaymentsAsync()
        {
            return await GetAsync<List<PaymentDto>>("payments") ?? new List<PaymentDto>();
        }

        public async Task<byte[]?> ExportPaymentsAsync(Guid? reservationId = null)
        {
             var endpoint = reservationId.HasValue 
                 ? $"exports/payments?reservationId={reservationId}" 
                 : "exports/payments";
             return await GetFileAsync(endpoint);
        }

        #endregion
    }

}
