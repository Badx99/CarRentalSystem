namespace CarRentalSystem.BackOffice.Models.DTOs
{
    public class VehicleTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PassengerCapacity { get; set; }
        public decimal BaseDailyRate { get; set; }
        public bool IsActive { get; set; }
    }
}
