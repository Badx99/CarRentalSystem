using CarRentalSystem.BackOffice.Models.DTOs;

namespace CarRentalSystem.BackOffice.Services
{
    public interface IApiClient
    {
        // Authentication
        Task<LoginResponse?> LoginAsync(LoginRequest request);

        // Dashboard
        Task<DashboardStatsDto?> GetDashboardStatsAsync();

        // Vehicles
        Task<List<VehicleDto>> GetVehiclesAsync();
        Task<VehicleDto?> GetVehicleByIdAsync(Guid id);
        Task<VehicleDto?> CreateVehicleAsync(CreateVehicleRequest request);
        Task<bool> UpdateVehicleAsync(Guid id, UpdateVehicleRequest request);
        Task<bool> UpdateVehicleStatusAsync(Guid id, string status);
        Task<bool> DeleteVehicleAsync(Guid id);
        Task<byte[]?> ExportVehiclesAsync();

        // Vehicle Types
        Task<List<VehicleTypeDto>> GetVehicleTypesAsync();
        Task<VehicleTypeDto?> GetVehicleTypeByIdAsync(Guid id);
        Task<VehicleTypeDto?> CreateVehicleTypeAsync(CreateVehicleTypeRequest request);
        Task<bool> UpdateVehicleTypeAsync(Guid id, UpdateVehicleTypeRequest request);
        Task<bool> DeleteVehicleTypeAsync(Guid id);

        // Reservations
        Task<List<ReservationDto>> GetReservationsAsync();
        Task<GetReservationsByCustomerResponse?> GetReservationsByCustomerAsync(Guid customerId);
        Task<ReservationDto?> GetReservationByIdAsync(Guid id);
        Task<bool> ConfirmReservationAsync(Guid id);
        Task<bool> StartRentalAsync(Guid id);
        Task<bool> CompleteReservationAsync(Guid id, int finalMileage);
        Task<bool> CancelReservationAsync(Guid id);

        Task<byte[]?> DownloadReceiptAsync(Guid id);
        Task<byte[]?> ExportReservationsAsync();

        // Customers
        Task<List<CustomerDto>> GetCustomersAsync();
        Task<CustomerDto?> GetCustomerByIdAsync(Guid id);

        // Employees (Admin only)
        Task<List<EmployeeDto>> GetEmployeesAsync();
        Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id);
        Task<EmployeeDto?> CreateEmployeeAsync(CreateEmployeeRequest request);
        Task<bool> UpdateEmployeeAsync(Guid id, UpdateEmployeeRequest request);
        Task<bool> DeleteEmployeeAsync(Guid id);


        
        // Payments
        Task<List<PaymentDto>> GetPaymentsAsync();
        Task<byte[]?> ExportPaymentsAsync(Guid? reservationId = null);

        // Token management
        void SetAuthToken(string token);
        void ClearAuthToken();

        // Event for unauthorized responses
        event Action? OnUnauthorized;
    }

    #region Request DTOs

    public class CreateVehicleRequest
    {
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int Mileage { get; set; }
        public Guid VehicleTypeId { get; set; }
        public decimal? DailyRate { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdateVehicleStatusCommand
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class UpdateVehicleRequest
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Color { get; set; } = string.Empty;
        public int Mileage { get; set; }
        public Guid VehicleTypeId { get; set; }
        public decimal? DailyRate { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class CreateVehicleTypeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PassengerCapacity { get; set; }
        public decimal BaseDailyRate { get; set; }
    }

    public class UpdateVehicleTypeRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PassengerCapacity { get; set; }
        public decimal BaseDailyRate { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateEmployeeRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateTime? HireDate { get; set; }
    }

    public class UpdateEmployeeRequest
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public bool IsActive { get; set; }
    }
    #endregion

}