using CarRentalSystem.BackOffice.Helpers;
using CarRentalSystem.BackOffice.Models.DTOs;
using CarRentalSystem.BackOffice.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CarRentalSystem.BackOffice.ViewModels
{
    public class VehicleTypesViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;

        #region Collections
        private ObservableCollection<VehicleTypeDto> _vehicleTypes = new();
        #endregion

        #region Selection and State
        private VehicleTypeDto? _selectedVehicleType;
        private bool _isLoading;
        private bool _isEditMode;
        private bool _isFormVisible;
        private string _validationMessage = string.Empty;
        #endregion

        #region Form Properties
        private string _name = string.Empty;
        private string _description = string.Empty;
        private int _passengerCapacity;
        private decimal _baseDailyRate;
        private bool _isActive = true;
        #endregion

        public VehicleTypesViewModel(IApiClient apiClient)
        {
            _apiClient = apiClient;

            // Commands
            LoadCommand = new RelayCommand(async _ => await LoadVehicleTypesAsync());
            RefreshCommand = new RelayCommand(async _ => await LoadVehicleTypesAsync());
            AddCommand = new RelayCommand(_ => ShowAddForm());
            EditCommand = new RelayCommand(_ => ShowEditForm(), _ => SelectedVehicleType != null);
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedVehicleType != null);
            CancelCommand = new RelayCommand(_ => HideForm());
        }

        #region Public Properties

        public ObservableCollection<VehicleTypeDto> VehicleTypes
        {
            get => _vehicleTypes;
            set => SetProperty(ref _vehicleTypes, value);
        }

        public VehicleTypeDto? SelectedVehicleType
        {
            get => _selectedVehicleType;
            set
            {
                if (SetProperty(ref _selectedVehicleType, value))
                {
                    OnPropertyChanged(nameof(HasSelection));
                    CommandManager.InvalidateRequerySuggested();
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

        public string ValidationMessage
        {
            get => _validationMessage;
            set => SetProperty(ref _validationMessage, value);
        }

        public bool HasSelection => SelectedVehicleType != null;
        public string FormTitle => IsEditMode ? "Edit Vehicle Type" : "Add Vehicle Type";

        // Form fields
        public string Name
        {
            get => _name;
            set { SetProperty(ref _name, value); ValidateForm(); }
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public int PassengerCapacity
        {
            get => _passengerCapacity;
            set { SetProperty(ref _passengerCapacity, value); ValidateForm(); }
        }

        public decimal BaseDailyRate
        {
            get => _baseDailyRate;
            set { SetProperty(ref _baseDailyRate, value); ValidateForm(); }
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        #endregion

        #region Commands

        public ICommand LoadCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Public Methods

        public async Task LoadVehicleTypesAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                var types = await _apiClient.GetVehicleTypesAsync();
                VehicleTypes = new ObservableCollection<VehicleTypeDto>(types);
            }
            catch (Exception ex)
            {
                ShowError($"Error loading vehicle types: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Private Methods

        private void ShowAddForm()
        {
            IsEditMode = false;
            ClearForm();
            IsFormVisible = true;
        }

        private void ShowEditForm()
        {
            if (SelectedVehicleType == null) return;

            IsEditMode = true;
            PopulateFormFromSelection();
            IsFormVisible = true;
        }

        private void HideForm()
        {
            IsFormVisible = false;
            ClearForm();
        }

        private void ClearForm()
        {
            Name = string.Empty;
            Description = string.Empty;
            PassengerCapacity = 0;
            BaseDailyRate = 0;
            IsActive = true;
            ValidationMessage = string.Empty;
        }

        private void PopulateFormFromSelection()
        {
            if (SelectedVehicleType == null) return;

            Name = SelectedVehicleType.Name;
            Description = SelectedVehicleType.Description;
            PassengerCapacity = SelectedVehicleType.PassengerCapacity;
            BaseDailyRate = SelectedVehicleType.BaseDailyRate;
            IsActive = SelectedVehicleType.IsActive;
            ValidationMessage = string.Empty;
        }

        private void ValidateForm()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("Name is required");

            if (PassengerCapacity <= 0)
                errors.Add("Passenger capacity must be greater than 0");

            if (BaseDailyRate <= 0)
                errors.Add("Base daily rate must be greater than 0");

            ValidationMessage = string.Join(", ", errors);
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   PassengerCapacity > 0 &&
                   BaseDailyRate > 0 &&
                   !IsLoading;
        }

        private async Task SaveAsync()
        {
            if (!CanSave()) return;

            try
            {
                IsLoading = true;

                if (IsEditMode && SelectedVehicleType != null)
                {
                    var request = new UpdateVehicleTypeRequest
                    {
                        Id = SelectedVehicleType.Id,
                        Name = Name,
                        Description = Description,
                        PassengerCapacity = PassengerCapacity,
                        BaseDailyRate = BaseDailyRate,
                        IsActive = IsActive
                    };

                    var success = await _apiClient.UpdateVehicleTypeAsync(SelectedVehicleType.Id, request);
                    if (success)
                    {
                        ShowMessage("Vehicle type updated successfully.", "Success");
                        HideForm();
                        await LoadVehicleTypesAsync();
                    }
                    else
                    {
                        ShowError("Failed to update vehicle type.");
                    }
                }
                else
                {
                    var request = new CreateVehicleTypeRequest
                    {
                        Name = Name,
                        Description = Description,
                        PassengerCapacity = PassengerCapacity,
                        BaseDailyRate = BaseDailyRate
                    };

                    var result = await _apiClient.CreateVehicleTypeAsync(request);
                    if (result != null)
                    {
                        ShowMessage("Vehicle type created successfully.", "Success");
                        HideForm();
                        await LoadVehicleTypesAsync();
                    }
                    else
                    {
                        ShowError("Failed to create vehicle type.");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error saving vehicle type: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedVehicleType == null) return;

            if (!ShowConfirmation($"Are you sure you want to delete '{SelectedVehicleType.Name}'?\n\nThis cannot be undone."))
                return;

            try
            {
                IsLoading = true;
                var success = await _apiClient.DeleteVehicleTypeAsync(SelectedVehicleType.Id);

                if (success)
                {
                    ShowMessage("Vehicle type deleted successfully.", "Success");
                    await LoadVehicleTypesAsync();
                }
                else
                {
                    ShowError("Failed to delete vehicle type. It may be in use by vehicles.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting vehicle type: {ex.Message}");
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
            MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private bool ShowConfirmation(string message)
        {
            var result = MessageBox.Show(message, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        #endregion
    }
}
