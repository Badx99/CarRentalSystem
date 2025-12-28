using CarRentalSystem.BackOffice.Helpers;
using CarRentalSystem.BackOffice.Models.DTOs;
using CarRentalSystem.BackOffice.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CarRentalSystem.BackOffice.ViewModels
{
    public class VehiclesViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;

        #region Collections
        private ObservableCollection<VehicleDto> _vehicles = new();
        private ObservableCollection<VehicleDto> _allVehicles = new();
        private List<VehicleTypeDto> _vehicleTypes = new();
        #endregion

        #region Selection and State
        private VehicleDto? _selectedVehicle;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private bool _isEditMode;
        private bool _isFormVisible;
        private string _errorMessage = string.Empty;
        private string _validationMessage = string.Empty;
        #endregion

        #region Filter and Static Data
        public List<string> VehicleStatuses { get; } = new()
        {
            "Available", "Rented", "Maintenance", "OutofService"
        };
        #endregion

        #region Form Properties
        private Guid _editingVehicleId;
        private string _brand = string.Empty;
        private string _model = string.Empty;
        private int _year = DateTime.Now.Year;
        private string _licensePlate = string.Empty;
        private string _color = string.Empty;
        private int _mileage;
        private decimal _dailyRate;
        private VehicleTypeDto? _selectedVehicleType;
        private string _status = "Available";
        private string _imageUrl = string.Empty;
        private System.Windows.Media.Imaging.BitmapImage? _previewImage;
        #endregion

        public VehiclesViewModel(IApiClient apiClient)
        {
            _apiClient = apiClient;

            // Commands
            LoadVehiclesCommand = new RelayCommand(async _ => await LoadVehiclesAsync());
            RefreshCommand = new RelayCommand(async _ => await LoadVehiclesAsync());
            AddCommand = new RelayCommand(_ => StartAddMode());
            EditCommand = new RelayCommand(_ => StartEditMode(), _ => SelectedVehicle != null);
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => CancelEdit());
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedVehicle != null);
            PreviewImageCommand = new RelayCommand(async _ => await PreviewImageAsync(), _ => !string.IsNullOrWhiteSpace(ImageUrl));
            UpdateStatusCommand = new RelayCommand(async param => await UpdateStatusAsync(param as VehicleDto));
            ExportCommand = new RelayCommand(async _ => await ExportAsync());
        }

        #region Public Properties - Collections

        public ObservableCollection<VehicleDto> Vehicles
        {
            get => _vehicles;
            set => SetProperty(ref _vehicles, value);
        }

        public List<VehicleTypeDto> VehicleTypes
        {
            get => _vehicleTypes;
            set => SetProperty(ref _vehicleTypes, value);
        }

        public VehicleDto? SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                if (SetProperty(ref _selectedVehicle, value))
                {
                    // Notify commands to re-evaluate CanExecute
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        #endregion

        #region Public Properties - State

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterVehicles();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public bool IsFormVisible
        {
            get => _isFormVisible;
            set => SetProperty(ref _isFormVisible, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            set => SetProperty(ref _validationMessage, value);
        }

        public string FormTitle => IsEditMode ? "Edit Vehicle" : "Add New Vehicle";

        #endregion

        #region Public Properties - Form

        public string Brand
        {
            get => _brand;
            set => SetProperty(ref _brand, value);
        }

        public string Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

        public int Year
        {
            get => _year;
            set => SetProperty(ref _year, value);
        }

        public string LicensePlate
        {
            get => _licensePlate;
            set => SetProperty(ref _licensePlate, value);
        }

        public string Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public int Mileage
        {
            get => _mileage;
            set => SetProperty(ref _mileage, value);
        }

        public decimal DailyRate
        {
            get => _dailyRate;
            set => SetProperty(ref _dailyRate, value);
        }

        public VehicleTypeDto? SelectedVehicleType
        {
            get => _selectedVehicleType;
            set => SetProperty(ref _selectedVehicleType, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        #endregion

        #region Commands

        public ICommand LoadVehiclesCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ICommand DeleteCommand { get; }
        public ICommand UpdateStatusCommand { get; }
        public ICommand ExportCommand { get; }

        #endregion

        #region Public Methods

        public async Task LoadVehiclesAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Load vehicles and vehicle types in parallel
                var vehiclesTask = _apiClient.GetVehiclesAsync();
                var typesTask = _apiClient.GetVehicleTypesAsync();

                await Task.WhenAll(vehiclesTask, typesTask);

                var vehicles = await vehiclesTask;
                VehicleTypes = await typesTask;

                _allVehicles = new ObservableCollection<VehicleDto>(vehicles);
                FilterVehicles();
            }
            catch (Exception ex)
            {
                ShowError($"Error loading vehicles: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Private Methods - Filtering

        private void FilterVehicles()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Vehicles = new ObservableCollection<VehicleDto>(_allVehicles);
            }
            else
            {
                var filtered = _allVehicles.Where(v =>
                    v.Brand.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    v.Model.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    v.LicensePlate.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    v.Color.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    v.Status.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    v.VehicleTypeName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                Vehicles = new ObservableCollection<VehicleDto>(filtered);
            }
        }

        #endregion

        #region Private Methods - Form Operations

        private void StartAddMode()
        {
            ClearForm();
            IsEditMode = false;
            IsFormVisible = true;
            OnPropertyChanged(nameof(FormTitle));
        }

        private void StartEditMode()
        {
            if (SelectedVehicle == null) return;

            _editingVehicleId = SelectedVehicle.Id;
            Brand = SelectedVehicle.Brand;
            Model = SelectedVehicle.Model;
            Year = SelectedVehicle.Year;
            LicensePlate = SelectedVehicle.LicensePlate;
            Color = SelectedVehicle.Color;
            Mileage = SelectedVehicle.Mileage;
            DailyRate = SelectedVehicle.DailyRate;
            ImageUrl = SelectedVehicle.ImageUrl ?? string.Empty;
            Status = SelectedVehicle.Status;
            SelectedVehicleType = VehicleTypes.FirstOrDefault(t => t.Id == SelectedVehicle.VehicleTypeId);

            IsEditMode = true;
            IsFormVisible = true;
            OnPropertyChanged(nameof(FormTitle));
        }

        private void CancelEdit()
        {
            ClearForm();
            IsFormVisible = false;
            IsEditMode = false;
            ValidationMessage = string.Empty;
        }

        private void ClearForm()
        {
            _editingVehicleId = Guid.Empty;
            Brand = string.Empty;
            Model = string.Empty;
            Year = DateTime.Now.Year;
            LicensePlate = string.Empty;
            Color = string.Empty;
            Mileage = 0;
            DailyRate = 0;
            ImageUrl = string.Empty;
            Status = "Available";
            SelectedVehicleType = VehicleTypes.FirstOrDefault();
            ValidationMessage = string.Empty;
        }

        private bool CanSave()
        {
            return !IsLoading && IsFormVisible;
        }

        private bool ValidateForm()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Brand))
                errors.Add("Brand is required");

            if (string.IsNullOrWhiteSpace(Model))
                errors.Add("Model is required");

            if (Year < 1900 || Year > DateTime.Now.Year + 1)
                errors.Add($"Year must be between 1900 and {DateTime.Now.Year + 1}");

            if (string.IsNullOrWhiteSpace(LicensePlate))
                errors.Add("License plate is required");

            if (string.IsNullOrWhiteSpace(Color))
                errors.Add("Color is required");

            if (Mileage < 0)
                errors.Add("Mileage cannot be negative");

            if(DailyRate < 0)
                errors.Add("Rate cannot be negative");

            if (SelectedVehicleType == null)
                errors.Add("Vehicle type is required");

            if (errors.Any())
            {
                ValidationMessage = string.Join("\n", errors);
                return false;
            }

            ValidationMessage = string.Empty;
            return true;
        }

        private async Task UpdateStatusAsync(VehicleDto? vehicle)
        {
            if (vehicle == null) return;

            try
            {
                // This would be called from the DataGrid when a ComboBox selection changes
                // Selection changes are usually handled via binding or a specific event/command
                IsLoading = true;
                var success = await _apiClient.UpdateVehicleStatusAsync(vehicle.Id, vehicle.Status);
                
                if (success)
                {
                    ShowMessage($"Status for {vehicle.Brand} {vehicle.Model} updated to {vehicle.Status}.", "Status Updated");
                    await LoadVehiclesAsync();
                }
                else
                {
                    ShowError("Failed to update vehicle status.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error updating status: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveAsync()
        {
            if (!ValidateForm()) return;

            try
            {
                IsLoading = true;
                bool success;

                if (IsEditMode)
                {
                    var request = new UpdateVehicleRequest
                    {
                        Id = _editingVehicleId,
                        Brand = Brand.Trim(),
                        Model = Model.Trim(),
                        Year = Year,
                        Color = Color.Trim(),
                        Mileage = Mileage,
                        DailyRate = DailyRate,
                        VehicleTypeId = SelectedVehicleType!.Id,
                        ImageUrl = string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl.Trim()
                    };
                    success = await _apiClient.UpdateVehicleAsync(_editingVehicleId, request);

                    if (success && Status != SelectedVehicle?.Status)
                    {
                        await _apiClient.UpdateVehicleStatusAsync(_editingVehicleId, Status);
                    }

                    if (success)
                    {
                        ShowMessage("Vehicle updated successfully!", "Success");
                    }
                }
                else
                {
                    var request = new CreateVehicleRequest
                    {
                        Brand = Brand.Trim(),
                        Model = Model.Trim(),
                        Year = Year,
                        LicensePlate = LicensePlate.Trim(),
                        Color = Color.Trim(),
                        Mileage = Mileage,
                        DailyRate = DailyRate,
                        VehicleTypeId = SelectedVehicleType!.Id,
                        ImageUrl = string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl.Trim()
                    };
                    var result = await _apiClient.CreateVehicleAsync(request);
                    success = result != null;

                    if (success)
                    {
                        ShowMessage("Vehicle created successfully!", "Success");
                    }
                }

                if (success)
                {
                    CancelEdit();
                    await LoadVehiclesAsync();
                }
                else
                {
                    ShowError("Failed to save vehicle. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error saving vehicle: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedVehicle == null) return;

            if (!ShowConfirmation($"Are you sure you want to delete {SelectedVehicle.Brand} {SelectedVehicle.Model} ({SelectedVehicle.LicensePlate})?"))
                return;

            try
            {
                IsLoading = true;
                var success = await _apiClient.DeleteVehicleAsync(SelectedVehicle.Id);

                if (success)
                {
                    ShowMessage("Vehicle deleted successfully!", "Success");
                    await LoadVehiclesAsync();
                }
                else
                {
                    ShowError("Failed to delete vehicle. It may be associated with active reservations.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting vehicle: {ex.Message}");
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
                var fileBytes = await _apiClient.ExportVehiclesAsync();
                
                if (fileBytes == null || fileBytes.Length == 0)
                {
                    ShowError("Failed to export vehicles or no data available.");
                    return;
                }

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    FileName = $"Vehicles_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    await System.IO.File.WriteAllBytesAsync(saveFileDialog.FileName, fileBytes);
                    ShowMessage("Vehicles exported successfully!", "Export Complete");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error exporting vehicles: {ex.Message}");
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
            var result = MessageBox.Show(message, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        #endregion

        #region Image Preview

        public System.Windows.Media.Imaging.BitmapImage? PreviewImage
        {
            get => _previewImage;
            set => SetProperty(ref _previewImage, value);
        }

        public ICommand PreviewImageCommand { get; private set; }

        private async Task PreviewImageAsync()
        {
            var image = await LoadImageFromUrl(ImageUrl);
            if (image != null)
            {
                PreviewImage = image;
            }
        }

        private async Task<System.Windows.Media.Imaging.BitmapImage?> LoadImageFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                ShowError("Image URL is empty.");
                return null;
            }

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                ShowError("Invalid URL format. URL must start with http:// or https://");
                return null;
            }

            try
            {
                using var httpClient = new System.Net.Http.HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                var imageData = await httpClient.GetByteArrayAsync(url);

                var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
                using (var memStream = new System.IO.MemoryStream(imageData))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memStream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                }
                return bitmapImage;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load image: {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}