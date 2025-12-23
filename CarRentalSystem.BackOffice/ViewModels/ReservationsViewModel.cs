using CarRentalSystem.BackOffice.Helpers;
using CarRentalSystem.BackOffice.Models.DTOs;
using CarRentalSystem.BackOffice.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace CarRentalSystem.BackOffice.ViewModels
{
    public class ReservationsViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;

        #region Collections
        private ObservableCollection<ReservationDto> _reservations = new();
        private ObservableCollection<ReservationDto> _allReservations = new();
        #endregion

        #region Selection and State
        private ReservationDto? _selectedReservation;
        private string _searchText = string.Empty;
        private string _statusFilter = "All";
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        #endregion

        #region Filter Options
        public List<string> StatusFilterOptions { get; } = new()
        {
            "All", "Pending", "Confirmed", "InProgress", "Completed", "Cancelled"
        };
        #endregion

        public ReservationsViewModel(IApiClient apiClient)
        {
            _apiClient = apiClient;

            // Commands
            LoadReservationsCommand = new RelayCommand(async _ => await LoadReservationsAsync());
            RefreshCommand = new RelayCommand(async _ => await LoadReservationsAsync());
            ConfirmCommand = new RelayCommand(async res => await ConfirmReservationAsync(res as ReservationDto), CanConfirm);
            StartRentalCommand = new RelayCommand(async res => await StartRentalAsync(res as ReservationDto), CanStart);
            CompleteCommand = new RelayCommand(async res => await CompleteReservationAsync(res as ReservationDto), CanComplete);
            CancelCommand = new RelayCommand(async res => await CancelReservationAsync(res as ReservationDto), CanCancel);
            DownloadReceiptCommand = new RelayCommand(async res => await DownloadReceiptAsync(res as ReservationDto), CanDownloadReceipt);
        }

        #region Public Properties

        public ObservableCollection<ReservationDto> Reservations
        {
            get => _reservations;
            set => SetProperty(ref _reservations, value);
        }

        public ReservationDto? SelectedReservation
        {
            get => _selectedReservation;
            set
            {
                if (SetProperty(ref _selectedReservation, value))
                {
                    OnPropertyChanged(nameof(HasSelection));
                    OnPropertyChanged(nameof(CanShowConfirm));
                    OnPropertyChanged(nameof(CanShowStart));
                    OnPropertyChanged(nameof(CanShowComplete));
                    OnPropertyChanged(nameof(CanShowCancel));
                    OnPropertyChanged(nameof(CanShowReceipt));
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
                    FilterReservations();
                }
            }
        }

        public string StatusFilter
        {
            get => _statusFilter;
            set
            {
                if (SetProperty(ref _statusFilter, value))
                {
                    FilterReservations();
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

        // Helper properties for UI visibility
        public bool HasSelection => SelectedReservation != null;
        public bool CanShowConfirm => SelectedReservation?.Status == "Pending";
        public bool CanShowStart => SelectedReservation?.Status == "Confirmed";
        public bool CanShowComplete => SelectedReservation?.Status == "InProgress";
        public bool CanShowCancel => SelectedReservation != null && 
            SelectedReservation.Status != "Completed" && SelectedReservation.Status != "Cancelled";
        public bool CanShowReceipt => SelectedReservation?.Status == "Completed";

        #endregion

        #region Commands

        public ICommand LoadReservationsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand StartRentalCommand { get; }
        public ICommand CompleteCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DownloadReceiptCommand { get; }

        #endregion

        #region CanExecute Methods

        private bool CanConfirm(object? param)
        {
            var reservation = param as ReservationDto ?? SelectedReservation;
            return reservation?.Status == "Pending" && !IsLoading;
        }

        private bool CanStart(object? param)
        {
            var reservation = param as ReservationDto ?? SelectedReservation;
            return reservation?.Status == "Confirmed" && !IsLoading;
        }

        private bool CanComplete(object? param)
        {
            var reservation = param as ReservationDto ?? SelectedReservation;
            return reservation?.Status == "InProgress" && !IsLoading;
        }

        private bool CanCancel(object? param)
        {
            var reservation = param as ReservationDto ?? SelectedReservation;
            return reservation != null && 
                   reservation.Status != "Completed" && 
                   reservation.Status != "Cancelled" && 
                   !IsLoading;
        }

        private bool CanDownloadReceipt(object? param)
        {
            var reservation = param as ReservationDto ?? SelectedReservation;
            return reservation?.Status == "Completed" && !IsLoading;
        }

        #endregion

        #region Public Methods

        public async Task LoadReservationsAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var reservations = await _apiClient.GetReservationsAsync();
                _allReservations = new ObservableCollection<ReservationDto>(reservations);
                FilterReservations();
            }
            catch (Exception ex)
            {
                ShowError($"Error loading reservations: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Private Methods - Filtering

        private void FilterReservations()
        {
            var filtered = _allReservations.AsEnumerable();

            // Apply status filter
            if (StatusFilter != "All")
            {
                filtered = filtered.Where(r => r.Status == StatusFilter);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(r =>
                    r.CustomerName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.CustomerEmail.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.VehicleBrand.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.VehicleModel.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.VehicleLicensePlate.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            Reservations = new ObservableCollection<ReservationDto>(filtered);
        }

        #endregion

        #region Private Methods - Status Actions

        private async Task ConfirmReservationAsync(ReservationDto? reservation)
        {
            reservation ??= SelectedReservation;
            if (reservation == null || reservation.Status != "Pending") return;

            try
            {
                IsLoading = true;
                var success = await _apiClient.ConfirmReservationAsync(reservation.Id);

                if (success)
                {
                    ShowMessage($"Reservation for {reservation.CustomerName} has been confirmed.", "Reservation Confirmed");
                    await LoadReservationsAsync();
                }
                else
                {
                    ShowError("Failed to confirm reservation. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error confirming reservation: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task StartRentalAsync(ReservationDto? reservation)
        {
            reservation ??= SelectedReservation;
            if (reservation == null || reservation.Status != "Confirmed") return;

            try
            {
                IsLoading = true;
                var success = await _apiClient.StartRentalAsync(reservation.Id);

                if (success)
                {
                    ShowMessage($"Rental started for {reservation.CustomerName}.\nVehicle: {reservation.VehicleBrand} {reservation.VehicleModel}", "Rental Started");
                    await LoadReservationsAsync();
                }
                else
                {
                    ShowError("Failed to start rental. Please verify the vehicle is available.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error starting rental: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CompleteReservationAsync(ReservationDto? reservation)
        {
            reservation ??= SelectedReservation;
            if (reservation == null || reservation.Status != "InProgress") return;

            // Show input dialog for final mileage
            var mileageInput = ShowInputDialog(
                "Enter Final Mileage",
                "Please enter the vehicle's current mileage:",
                "0");

            if (mileageInput == null) return; // User cancelled

            if (!int.TryParse(mileageInput, out int finalMileage) || finalMileage < 0)
            {
                ShowError("Please enter a valid mileage value.");
                return;
            }

            try
            {
                IsLoading = true;
                var success = await _apiClient.CompleteReservationAsync(reservation.Id, finalMileage);

                if (success)
                {
                    ShowMessage($"Reservation completed successfully!\nCustomer: {reservation.CustomerName}\nTotal Amount: {reservation.TotalAmount:C2}", "Reservation Completed");
                    await LoadReservationsAsync();
                }
                else
                {
                    ShowError("Failed to complete reservation. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error completing reservation: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CancelReservationAsync(ReservationDto? reservation)
        {
            reservation ??= SelectedReservation;
            if (reservation == null || reservation.Status == "Completed" || reservation.Status == "Cancelled") return;

            if (!ShowConfirmation($"Are you sure you want to cancel the reservation for {reservation.CustomerName}?\n\nVehicle: {reservation.VehicleBrand} {reservation.VehicleModel}\nDates: {reservation.StartDate:d} - {reservation.EndDate:d}"))
                return;

            try
            {
                IsLoading = true;
                var success = await _apiClient.CancelReservationAsync(reservation.Id);

                if (success)
                {
                    ShowMessage("Reservation has been cancelled.", "Reservation Cancelled");
                    await LoadReservationsAsync();
                }
                else
                {
                    ShowError("Failed to cancel reservation. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error cancelling reservation: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DownloadReceiptAsync(ReservationDto? reservation)
        {
            reservation ??= SelectedReservation;
            if (reservation == null || reservation.Status != "Completed") return;

            try
            {
                IsLoading = true;
                var receiptData = await _apiClient.DownloadReceiptAsync(reservation.Id);

                if (receiptData == null)
                {
                    ShowError("Receipt is not available for this reservation.");
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    FileName = $"Receipt_{reservation.CustomerName.Replace(" ", "_")}_{reservation.StartDate:yyyyMMdd}",
                    DefaultExt = ".pdf",
                    Filter = "PDF Documents (.pdf)|*.pdf|All Files (*.*)|*.*",
                    Title = "Save Receipt"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    await File.WriteAllBytesAsync(saveDialog.FileName, receiptData);
                    ShowMessage($"Receipt saved to:\n{saveDialog.FileName}", "Receipt Downloaded");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error downloading receipt: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
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

        private bool ShowConfirmation(string message)
        {
            var result = MessageBox.Show(message, "Confirm Action", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        private string? ShowInputDialog(string title, string prompt, string defaultValue)
        {
            // Simple input dialog using MessageBox with InputBox-like behavior
            // In a real app, you'd create a custom dialog window
            var inputWindow = new Window
            {
                Title = title,
                Width = 350,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.ToolWindow,
                ResizeMode = ResizeMode.NoResize
            };

            var panel = new System.Windows.Controls.StackPanel { Margin = new Thickness(15) };
            var label = new System.Windows.Controls.Label { Content = prompt };
            var textBox = new System.Windows.Controls.TextBox { Text = defaultValue, Margin = new Thickness(0, 5, 0, 15) };
            var buttonPanel = new System.Windows.Controls.StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            
            var okButton = new System.Windows.Controls.Button { Content = "OK", Width = 75, Margin = new Thickness(0, 0, 10, 0), IsDefault = true };
            var cancelButton = new System.Windows.Controls.Button { Content = "Cancel", Width = 75, IsCancel = true };

            string? result = null;
            okButton.Click += (s, e) => { result = textBox.Text; inputWindow.DialogResult = true; };
            cancelButton.Click += (s, e) => { inputWindow.DialogResult = false; };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            panel.Children.Add(label);
            panel.Children.Add(textBox);
            panel.Children.Add(buttonPanel);
            inputWindow.Content = panel;

            return inputWindow.ShowDialog() == true ? result : null;
        }

        #endregion
    }
}