using CarRentalSystem.BackOffice.Helpers;
using CarRentalSystem.BackOffice.Models.DTOs;
using CarRentalSystem.BackOffice.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CarRentalSystem.BackOffice.ViewModels
{
    public class CustomersViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;

        #region Collections
        private ObservableCollection<CustomerDto> _customers = new();
        private ObservableCollection<CustomerDto> _allCustomers = new();
        #endregion

        #region Selection and State
        private CustomerDto? _selectedCustomer;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        #endregion

        public event Action<Guid>? RequestViewReservations;


        public CustomersViewModel(IApiClient apiClient)
        {
            _apiClient = apiClient;

            // Commands
            LoadCustomersCommand = new RelayCommand(async _ => await LoadCustomersAsync());
            RefreshCommand = new RelayCommand(async _ => await LoadCustomersAsync());
            ViewReservationsCommand = new RelayCommand(_ => ViewCustomerReservations(), _ => SelectedCustomer != null);
        }

        #region Public Properties

        public ObservableCollection<CustomerDto> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public CustomerDto? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    OnPropertyChanged(nameof(HasSelection));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterCustomers();
                }
            }
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

        public bool HasSelection => SelectedCustomer != null;

        #endregion

        #region Commands

        public ICommand LoadCustomersCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ViewReservationsCommand { get; }

        #endregion

        #region Public Methods

        public async Task LoadCustomersAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var customers = await _apiClient.GetCustomersAsync();
                _allCustomers = new ObservableCollection<CustomerDto>(customers);
                FilterCustomers();
            }
            catch (Exception ex)
            {
                ShowError($"Error loading customers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Private Methods

        private void FilterCustomers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Customers = new ObservableCollection<CustomerDto>(_allCustomers);
            }
            else
            {
                var filtered = _allCustomers.Where(c =>
                    c.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    c.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (c.PhoneNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.LicenseNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
                Customers = new ObservableCollection<CustomerDto>(filtered);
            }
        }

        private void ViewCustomerReservations()
        {
            if (SelectedCustomer == null) return;
            RequestViewReservations?.Invoke(SelectedCustomer.Id);
        }

        #endregion

        #region Dialog Helpers

        private void ShowMessage(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowError(string error)
        {
            ErrorMessage = error;
            MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion
    }
}
