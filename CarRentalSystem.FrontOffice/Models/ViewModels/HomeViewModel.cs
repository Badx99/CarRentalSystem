using CarRentalSystem.FrontOffice.Models.DTOs;

namespace CarRentalSystem.FrontOffice.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<VehicleDto> FeaturedVehicles { get; set; } = new();
        public int TotalVehicles { get; set; }
        public List<string> VehicleTypes { get; set; } = new();
    }
}
