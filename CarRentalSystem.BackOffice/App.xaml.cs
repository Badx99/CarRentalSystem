using CarRentalSystem.BackOffice.Models;
using CarRentalSystem.BackOffice.Services;
using CarRentalSystem.BackOffice.ViewModels;
using CarRentalSystem.BackOffice.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CarRentalSystem.BackOffice
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configure Dependency Injection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            // Show Login Window
            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register Services
            services.AddSingleton<IApiClient, ApiClient>();
            services.AddSingleton<UserSession>();

            // Register ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton<VehiclesViewModel>();
            services.AddSingleton<ReservationsViewModel>();
            services.AddSingleton<CustomersViewModel>();
            services.AddSingleton<VehicleTypesViewModel>();
            services.AddSingleton<EmployeesViewModel>();
            services.AddSingleton<PaymentsViewModel>();

            // Register Views
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();
        }

        public static T GetService<T>() where T : class
        {
            return _serviceProvider?.GetRequiredService<T>()
                ?? throw new InvalidOperationException($"Service {typeof(T)} not found");
        }
    }

    /// <summary>
    /// Converts a boolean to Visibility (inverse - true = Collapsed, false = Visible)
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// Converts a string to Visibility (Visible if not empty, Collapsed if empty)
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrEmpty(str))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
