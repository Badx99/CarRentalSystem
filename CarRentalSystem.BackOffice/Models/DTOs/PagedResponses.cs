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
}
