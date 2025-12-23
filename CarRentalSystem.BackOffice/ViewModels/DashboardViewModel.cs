using CarRentalSystem.BackOffice.Helpers;
using CarRentalSystem.BackOffice.Models.DTOs;
using CarRentalSystem.BackOffice.Services;
using System.Windows.Input;

namespace CarRentalSystem.BackOffice.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;

        private DashboardStatsDto? _stats;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        public DashboardViewModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
        }

        public DashboardStatsDto? Stats
        {
            get => _stats;
            set => SetProperty(ref _stats, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand RefreshCommand { get; }

        public async Task LoadDataAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var stats = await _apiClient.GetDashboardStatsAsync();
                if (stats != null)
                {
                    Stats = stats;
                }
                else
                {
                    // Create default stats if API fails
                    Stats = new DashboardStatsDto();
                    ErrorMessage = "Failed to load dashboard data.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading dashboard: {ex.Message}";
                Stats = new DashboardStatsDto();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}