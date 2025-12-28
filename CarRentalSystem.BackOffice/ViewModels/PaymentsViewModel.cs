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
    public class PaymentsViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;

        private ObservableCollection<PaymentDto> _payments = new();
        private PaymentDto? _selectedPayment;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        public PaymentsViewModel(IApiClient apiClient)
        {
            _apiClient = apiClient;

            LoadPaymentsCommand = new RelayCommand(async _ => await LoadPaymentsAsync());
            RefreshCommand = new RelayCommand(async _ => await LoadPaymentsAsync());
            ExportCommand = new RelayCommand(async _ => await ExportAsync());
        }

        #region Public Properties

        public ObservableCollection<PaymentDto> Payments
        {
            get => _payments;
            set => SetProperty(ref _payments, value);
        }

        public PaymentDto? SelectedPayment
        {
            get => _selectedPayment;
            set => SetProperty(ref _selectedPayment, value);
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

        #endregion

        #region Commands

        public ICommand LoadPaymentsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ExportCommand { get; }

        #endregion

        #region Public Methods

        public async Task LoadPaymentsAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var payments = await _apiClient.GetPaymentsAsync();
                Payments = new ObservableCollection<PaymentDto>(payments);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading payments: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportAsync()
        {
            try
            {
                IsLoading = true;
                var fileBytes = await _apiClient.ExportPaymentsAsync();

                if (fileBytes == null || fileBytes.Length == 0)
                {
                    ErrorMessage = "Failed to export payments or no data available.";
                    MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    FileName = $"Payments_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    await File.WriteAllBytesAsync(saveFileDialog.FileName, fileBytes);
                    MessageBox.Show("Payments exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error exporting payments: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion
    }
}
