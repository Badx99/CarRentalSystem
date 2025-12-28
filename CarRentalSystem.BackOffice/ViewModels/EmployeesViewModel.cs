using CarRentalSystem.BackOffice.Helpers;
using CarRentalSystem.BackOffice.Models.DTOs;
using CarRentalSystem.BackOffice.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CarRentalSystem.BackOffice.ViewModels
{
    public class EmployeesViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;

        private ObservableCollection<EmployeeDto> _employees = new();
        private EmployeeDto? _selectedEmployee;
        private bool _isLoading;
        private bool _isEditing;
        private bool _isAddMode;
        private string _errorMessage = string.Empty;

        // Form fields
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _employeeNumber = string.Empty;
        private decimal _salary;
        private DateTime _hireDate = DateTime.Today;
        private bool _isActive = true;

        public EmployeesViewModel(IApiClient apiClient)
        {
            _apiClient = apiClient;

            // Commands
            LoadCommand = new RelayCommand(async _ => await LoadEmployeesAsync());
            AddCommand = new RelayCommand(_ => StartAddMode());
            EditCommand = new RelayCommand(_ => StartEditMode(), _ => SelectedEmployee != null);
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedEmployee != null && !IsEditing);
            CancelCommand = new RelayCommand(_ => CancelEdit());
        }

        #region Properties

        public ObservableCollection<EmployeeDto> Employees
        {
            get => _employees;
            set => SetProperty(ref _employees, value);
        }

        public EmployeeDto? SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                SetProperty(ref _selectedEmployee, value);
                ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public bool IsAddMode
        {
            get => _isAddMode;
            set => SetProperty(ref _isAddMode, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string FormTitle => IsAddMode ? "Add New Employee" : "Edit Employee";

        // Form Properties
        public string FirstName
        {
            get => _firstName;
            set { SetProperty(ref _firstName, value); ((RelayCommand)SaveCommand).RaiseCanExecuteChanged(); }
        }

        public string LastName
        {
            get => _lastName;
            set { SetProperty(ref _lastName, value); ((RelayCommand)SaveCommand).RaiseCanExecuteChanged(); }
        }

        public string Email
        {
            get => _email;
            set { SetProperty(ref _email, value); ((RelayCommand)SaveCommand).RaiseCanExecuteChanged(); }
        }

        public string Password
        {
            get => _password;
            set { SetProperty(ref _password, value); ((RelayCommand)SaveCommand).RaiseCanExecuteChanged(); }
        }

        public string EmployeeNumber
        {
            get => _employeeNumber;
            set { SetProperty(ref _employeeNumber, value); ((RelayCommand)SaveCommand).RaiseCanExecuteChanged(); }
        }

        public decimal Salary
        {
            get => _salary;
            set { SetProperty(ref _salary, value); ((RelayCommand)SaveCommand).RaiseCanExecuteChanged(); }
        }

        public DateTime HireDate
        {
            get => _hireDate;
            set => SetProperty(ref _hireDate, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        #endregion

        #region Commands

        public ICommand LoadCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Methods

        public async Task LoadEmployeesAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var employees = await _apiClient.GetEmployeesAsync();
                Employees = new ObservableCollection<EmployeeDto>(employees);
            }
            catch (ApiException apiEx)
            {
                ErrorMessage = apiEx.GetErrorMessage() ?? "Failed to load employees.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading employees: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void StartAddMode()
        {
            IsAddMode = true;
            IsEditing = true;
            ClearForm();
            HireDate = DateTime.Today;
            IsActive = true;
            OnPropertyChanged(nameof(FormTitle));
        }

        private void StartEditMode()
        {
            if (SelectedEmployee == null) return;

            IsAddMode = false;
            IsEditing = true;

            // Populate form
            FirstName = SelectedEmployee.FirstName;
            LastName = SelectedEmployee.LastName;
            Email = SelectedEmployee.Email;
            EmployeeNumber = SelectedEmployee.EmployeeNumber;
            Salary = SelectedEmployee.Salary;
            HireDate = SelectedEmployee.HireDate;
            IsActive = SelectedEmployee.IsActive;
            Password = string.Empty; // Don't show existing password

            OnPropertyChanged(nameof(FormTitle));
        }

        private void CancelEdit()
        {
            IsEditing = false;
            IsAddMode = false;
            ClearForm();
        }

        private void ClearForm()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
            EmployeeNumber = string.Empty;
            Salary = 0;
            HireDate = DateTime.Today;
            IsActive = true;
        }

        private bool CanSave()
        {
            if (!IsEditing) return false;
            if (string.IsNullOrWhiteSpace(FirstName)) return false;
            if (string.IsNullOrWhiteSpace(LastName)) return false;
            if (string.IsNullOrWhiteSpace(Email)) return false;
            if (string.IsNullOrWhiteSpace(EmployeeNumber)) return false;
            if (Salary <= 0) return false;
            if (IsAddMode && string.IsNullOrWhiteSpace(Password)) return false;
            return true;
        }

        private async Task SaveAsync()
        {
            if (!CanSave()) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                if (IsAddMode)
                {
                    var request = new CreateEmployeeRequest
                    {
                        FirstName = FirstName,
                        LastName = LastName,
                        Email = Email,
                        Password = Password,
                        EmployeeNumber = EmployeeNumber,
                        Salary = Salary,
                        HireDate = HireDate.ToUniversalTime()
                    };

                    var result = await _apiClient.CreateEmployeeAsync(request);
                    if (result != null)
                    {
                        MessageBox.Show($"Employee '{result.FirstName} {result.LastName}' created successfully!",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadEmployeesAsync();
                        CancelEdit();
                    }
                    else
                    {
                        ErrorMessage = "Failed to create employee.";
                    }
                }
                else
                {
                    if (SelectedEmployee == null) return;

                    var request = new UpdateEmployeeRequest
                    {
                        Id = SelectedEmployee.Id,
                        FirstName = FirstName,
                        LastName = LastName,
                        Email = Email,
                        EmployeeNumber = EmployeeNumber,
                        Salary = Salary,
                        IsActive = IsActive
                    };

                    var success = await _apiClient.UpdateEmployeeAsync(SelectedEmployee.Id, request);
                    if (success)
                    {
                        MessageBox.Show("Employee updated successfully!",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadEmployeesAsync();
                        CancelEdit();
                    }
                    else
                    {
                        ErrorMessage = "Failed to update employee.";
                    }
                }
            }
            catch (ApiException apiEx)
            {
                ErrorMessage = apiEx.GetErrorMessage() ?? "Failed to save employee.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving employee: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedEmployee == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete employee '{SelectedEmployee.FullName}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var success = await _apiClient.DeleteEmployeeAsync(SelectedEmployee.Id);
                if (success)
                {
                    MessageBox.Show("Employee deleted successfully!",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadEmployeesAsync();
                }
                else
                {
                    ErrorMessage = "Failed to delete employee.";
                }
            }
            catch (ApiException apiEx)
            {
                ErrorMessage = apiEx.GetErrorMessage() ?? "Failed to delete employee.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting employee: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion
    }
}
