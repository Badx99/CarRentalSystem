using CarRentalSystem.FrontOffice.Models.DTOs;

namespace CarRentalSystem.FrontOffice.Models.ViewModels
{
    public class VehicleCatalogViewModel
    {
        public PagedResult<VehicleDto> Vehicles { get; set; } = new();
        public List<VehicleTypeDto> VehicleTypes { get; set; } = new();
        public string? SearchTerm { get; set; }
        public Guid? SelectedVehicleTypeId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
