namespace CarRentalSystem.BackOffice.Models.DTOs
{
    /// <summary>
    /// Wrapper for paginated vehicle response from API
    /// </summary>
    public class VehiclesPagedResponse
    {
        public List<VehicleDto> Vehicles { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Wrapper for paginated vehicle types response from API
    /// </summary>
    public class VehicleTypesPagedResponse
    {
        public List<VehicleTypeDto> VehicleTypes { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Wrapper for paginated reservations response from API
    /// </summary>
    public class ReservationsPagedResponse
    {
        public List<ReservationDto> Reservations { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Wrapper for paginated customers response from API
    /// </summary>
    public class CustomersPagedResponse
    {
        public List<CustomerDto> Customers { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Wrapper for paginated employees response from API
    /// </summary>
    public class EmployeesPagedResponse
    {
        public List<EmployeeDto> Employees { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Response wrapper for single vehicle by ID
    /// </summary>
    public class VehicleByIdResponse
    {
        public VehicleDto? Vehicle { get; set; }
    }

    public class CustomerReservationDto
    {
        public Guid Id { get; set; }
        public Guid VehicleId { get; set; }
        public string VehicleInfo { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RentalDays { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class GetReservationsByCustomerResponse
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public List<CustomerReservationDto> Reservations { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
