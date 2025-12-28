using CarRentalSystem.BackOffice.Helpers;
using CarRentalSystem.BackOffice.Models;
using System.Windows.Input;

namespace CarRentalSystem.BackOffice.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly UserSession _userSession;
        private readonly DashboardViewModel _dashboardViewModel;
        private readonly VehiclesViewModel _vehiclesViewModel;
        private readonly ReservationsViewModel _reservationsViewModel;
        private readonly CustomersViewModel _customersViewModel;
        private readonly VehicleTypesViewModel _vehicleTypesViewModel;
        private readonly EmployeesViewModel _employeesViewModel;
        private readonly PaymentsViewModel _paymentsViewModel;

        private BaseViewModel _currentViewModel;
        private string _currentViewName = "Dashboard";

        public MainViewModel(
            UserSession userSession,
            DashboardViewModel dashboardViewModel,
            VehiclesViewModel vehiclesViewModel,
            ReservationsViewModel reservationsViewModel,
            CustomersViewModel customersViewModel,
            VehicleTypesViewModel vehicleTypesViewModel,
            EmployeesViewModel employeesViewModel,
            PaymentsViewModel paymentsViewModel)
        {
            _userSession = userSession;
            _dashboardViewModel = dashboardViewModel;
            _vehiclesViewModel = vehiclesViewModel;
            _reservationsViewModel = reservationsViewModel;
            _customersViewModel = customersViewModel;
            _vehicleTypesViewModel = vehicleTypesViewModel;
            _employeesViewModel = employeesViewModel;
            _paymentsViewModel = paymentsViewModel;

            _currentViewModel = _dashboardViewModel;

            // Commands
            NavigateToDashboardCommand = new RelayCommand(_ => NavigateTo("Dashboard"));
            NavigateToVehiclesCommand = new RelayCommand(_ => NavigateTo("Vehicles"));
            NavigateToReservationsCommand = new RelayCommand(_ => NavigateTo("Reservations"));
            NavigateToCustomersCommand = new RelayCommand(_ => NavigateTo("Customers"));
            NavigateToVehicleTypesCommand = new RelayCommand(_ => NavigateTo("VehicleTypes"));
            NavigateToEmployeesCommand = new RelayCommand(_ => NavigateTo("Employees"));
            NavigateToPaymentsCommand = new RelayCommand(_ => NavigateTo("Payments"));
            LogoutCommand = new RelayCommand(_ => Logout());

            // Subscribe to navigation events
            _customersViewModel.RequestViewReservations += NavigateToCustomerReservations;
        }

        private void NavigateToCustomerReservations(Guid customerId)
        {
            _reservationsViewModel.CustomerIdFilter = customerId;
            NavigateTo("Reservations");
        }

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public string CurrentViewName
        {
            get => _currentViewName;
            set => SetProperty(ref _currentViewName, value);
        }

        public string UserFullName => _userSession.FullName;
        public string UserRole => _userSession.UserType;

        // Check if user is admin for conditional navigation items
        public bool IsAdmin => _userSession.UserType?.Equals("Administrator", StringComparison.OrdinalIgnoreCase) == true;

        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToVehiclesCommand { get; }
        public ICommand NavigateToReservationsCommand { get; }
        public ICommand NavigateToCustomersCommand { get; }
        public ICommand NavigateToVehicleTypesCommand { get; }
        public ICommand NavigateToEmployeesCommand { get; }
        public ICommand NavigateToPaymentsCommand { get; }
        public ICommand LogoutCommand { get; }

        private void NavigateTo(string viewName)
        {
            CurrentViewName = viewName;

            CurrentViewModel = viewName switch
            {
                "Dashboard" => _dashboardViewModel,
                "Vehicles" => _vehiclesViewModel,
                "Reservations" => _reservationsViewModel,
                "Customers" => _customersViewModel,
                "VehicleTypes" => _vehicleTypesViewModel,
                "Employees" => _employeesViewModel,
                "Payments" => _paymentsViewModel,
                _ => _dashboardViewModel
            };

            // Load data when navigating
            _ = LoadCurrentViewDataAsync();
        }

        private async Task LoadCurrentViewDataAsync()
        {
            if (CurrentViewModel is DashboardViewModel dashboard)
            {
                await dashboard.LoadDataAsync();
            }
            else if (CurrentViewModel is VehiclesViewModel vehicles)
            {
                await vehicles.LoadVehiclesAsync();
            }
            else if (CurrentViewModel is ReservationsViewModel reservations)
            {
                await reservations.LoadReservationsAsync();
            }
            else if (CurrentViewModel is CustomersViewModel customers)
            {
                await customers.LoadCustomersAsync();
            }
            else if (CurrentViewModel is VehicleTypesViewModel vehicleTypes)
            {
                await vehicleTypes.LoadVehicleTypesAsync();
            }
            else if (CurrentViewModel is EmployeesViewModel employees)
            {
                await employees.LoadEmployeesAsync();
            }
            else if (CurrentViewModel is PaymentsViewModel payments)
            {
                await payments.LoadPaymentsAsync();
            }
        }

        private void Logout()
        {
            // Clear session
            _userSession.Token = string.Empty;

            // Show login window and close main window
            var loginWindow = App.GetService<Views.LoginWindow>();
            loginWindow.Show();

            System.Windows.Application.Current.Windows
                .OfType<System.Windows.Window>()
                .FirstOrDefault(w => w.DataContext == this)?.Close();
        }

        public async Task InitializeAsync()
        {
            await _dashboardViewModel.LoadDataAsync();
        }
    }
}
