using CarRentalSystem.BackOffice.Helpers;
using CarRentalSystem.BackOffice.Models;
using CarRentalSystem.BackOffice.Models.DTOs;
using CarRentalSystem.BackOffice.Services;
using CarRentalSystem.BackOffice.Views;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;

namespace CarRentalSystem.BackOffice.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly UserSession _userSession;

        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public LoginViewModel(IApiClient apiClient, UserSession userSession)
        {
            _apiClient = apiClient;
            _userSession = userSession;

            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => CanLogin());
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoginCommand { get; }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !IsLoading;
        }

        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var request = new LoginRequest
                {
                    Email = Email.Trim(),
                    Password = Password
                };

                var response = await _apiClient.LoginAsync(request);

                if (response != null)
                {
                    // Check if user is Admin or Employee
                    if (response.UserType != "Administrator" && response.UserType != "Employee")
                    {
                        ErrorMessage = "Access denied. Only administrators and employees can access this application.";
                        return;
                    }

                    // Store session
                    _userSession.Token = response.Token;
                    _userSession.UserId = response.UserId;
                    _userSession.Email = response.Email;
                    _userSession.FullName = response.FullName;
                    _userSession.UserType = response.UserType;
                    _userSession.ExpiresAt = response.ExpiresAt;

                    // Set token in API client
                    _apiClient.SetAuthToken(response.Token);

                    // Open main window
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var mainWindow = App.GetService<MainWindow>();
                        mainWindow.Show();

                        // Close login window
                        Application.Current.Windows
                            .OfType<Window>()
                            .FirstOrDefault(w => w.DataContext == this)?.Close();
                    });
                }
                else
                {
                    ErrorMessage = "Invalid email or password.";
                }
            }
            catch (HttpRequestException)
            {
                ErrorMessage = "Unable to connect to server. Please ensure the API is running.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}

