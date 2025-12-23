namespace CarRentalSystem.FrontOffice.Models.DTOs
{
    public class VehicleDto
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string VehicleTypeName { get; set; } = string.Empty;
        public decimal DailyRate { get; set; }
        public int PassengerCapacity { get; set; }
        public int Mileage { get; set; }
    }

    public class VehicleDetailDto : VehicleDto
    {
        public string? Description { get; set; }
        public Guid VehicleTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SearchVehiclesQuery
    {
        public string? SearchTerm { get; set; }
        public Guid? VehicleTypeId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
