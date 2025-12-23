using CarRentalSystem.FrontOffice.Models.DTOs;

namespace CarRentalSystem.FrontOffice.Models.ViewModels
{
    public class BookingViewModel
    {
        public VehicleDto Vehicle { get; set; } = new();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RentalDays { get; set; }
        public decimal DailyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }

        public void Calculate()
        {
            if (EndDate > StartDate)
            {
                RentalDays = (EndDate - StartDate).Days;
                DailyRate = Vehicle.DailyRate;
                TotalAmount = RentalDays * DailyRate;
            }
        }
    }
}
