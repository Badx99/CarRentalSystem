namespace CarRentalSystem.FrontOffice.Models.DTOs;

public class ReservationDto
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string VehicleLicensePlate { get; set; } = string.Empty;
        public string VehicleTypeName { get; set; } = string.Empty;
        public string? VehicleImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

public class ReservationDetailDto : ReservationDto
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public Guid VehicleId { get; set; }
        public int RentalDays { get; set; }
        public string? Notes { get; set; }
    }

    
