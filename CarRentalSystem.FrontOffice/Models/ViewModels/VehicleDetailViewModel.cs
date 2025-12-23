using CarRentalSystem.Application.Features.Vehicles.Queries.GetVehicleById;
using CarRentalSystem.FrontOffice.Models.DTOs;

namespace CarRentalSystem.FrontOffice.Models.ViewModels
{
    public class VehicleDetailViewModel
    {
        public GetVehicleByIdResponse Vehicle { get; set; } = new();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsAvailable { get; set; }
        public decimal? TotalPrice { get; set; }
        public int RentalDays { get; set; }

        public void CalculateRentalDetails()
        {
            if (StartDate.HasValue && EndDate.HasValue && EndDate > StartDate)
            {
                RentalDays = (EndDate.Value - StartDate.Value).Days;
                TotalPrice = RentalDays * Vehicle.DailyRate;
            }
        }
    }
}
