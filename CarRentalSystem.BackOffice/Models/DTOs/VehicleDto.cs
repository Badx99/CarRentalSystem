namespace CarRentalSystem.BackOffice.Models.DTOs
{
    public class VehicleDto
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int Mileage { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string VehicleTypeName { get; set; } = string.Empty;
        public decimal DailyRate { get; set; }
        public Guid VehicleTypeId { get; set; }
    }
}
