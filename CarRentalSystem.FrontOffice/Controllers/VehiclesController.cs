using CarRentalSystem.FrontOffice.Models.DTOs;
using CarRentalSystem.FrontOffice.Models.ViewModels;
using CarRentalSystem.FrontOffice.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.FrontOffice.Controllers
{
    public class VehiclesController : Controller
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(ApiClient apiClient, ILogger<VehiclesController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Catalog(
            string? search, 
            Guid? vehicleTypeId, 
            decimal? minPrice, 
            decimal? maxPrice, 
            int page = 1)
        {
            var viewModel = new VehicleCatalogViewModel
            {
                SearchTerm = search,
                SelectedVehicleTypeId = vehicleTypeId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                CurrentPage = page,
                PageSize = 12,
                Vehicles = new PagedResult<VehicleDto> { Items = new List<VehicleDto>() } // Default empty
            };

            try
            {
                var query = new SearchVehiclesQuery
                {
                    SearchTerm = search,
                    VehicleTypeId = vehicleTypeId,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Page = page,
                    PageSize = 12,
                    Status = "Available"
                };

                var searchResult = await _apiClient.SearchVehiclesAsync(query);
                var vehicleTypes = await _apiClient.GetVehicleTypesAsync();

                viewModel.Vehicles = searchResult;
                viewModel.VehicleTypes = vehicleTypes ?? new List<VehicleTypeDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load vehicle catalog");
                TempData["Error"] = "Unable to load vehicles at this time. Please try again later.";
                
                // Try to at least load types if possible, or leave as empty
                viewModel.VehicleTypes = new List<VehicleTypeDto>();
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Details(Guid id, DateTime? startDate, DateTime? endDate)
        {
            try 
            {
                var vehicle = await _apiClient.GetVehicleByIdAsync(id);
                if (vehicle == null)
                {
                    return NotFound();
                }

                var viewModel = new VehicleDetailViewModel
                {
                    Vehicle = vehicle,
                    StartDate = startDate,
                    EndDate = endDate,
                    IsAvailable = true
                };

                if (startDate.HasValue && endDate.HasValue)
                {
                    if (startDate.Value < DateTime.Today || endDate.Value <= startDate.Value)
                    {
                        ModelState.AddModelError("", "Invalid date range");
                        viewModel.IsAvailable = false;
                    }
                    else
                    {
                        try 
                        {
                            var availableVehicles = await _apiClient.GetAvailableVehiclesAsync(startDate.Value, endDate.Value);
                            viewModel.IsAvailable = availableVehicles.Any(v => v.Id == id);
                            viewModel.CalculateRentalDetails();
                        }
                        catch(Exception ex)
                        {
                             _logger.LogError(ex, "Failed to check availability for vehicle {Id}", id);
                             TempData["Error"] = "Could not check real-time availability. Please try again.";
                        }
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load details for vehicle {Id}", id);
                TempData["Error"] = "Vehicle details unavailable.";
                return RedirectToAction(nameof(Catalog));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckAvailability(Guid vehicleId, DateTime startDate, DateTime endDate)
        {
            if (startDate < DateTime.Today || endDate <= startDate)
            {
                return Json(new { available = false, message = "Invalid dates" });
            }

            try
            {
                var vehicle = await _apiClient.GetVehicleByIdAsync(vehicleId);
                if (vehicle == null)
                {
                    return Json(new { available = false, message = "Vehicle not found" });
                }

                var availableVehicles = await _apiClient.GetAvailableVehiclesAsync(startDate, endDate);
                var isAvailable = availableVehicles.Any(v => v.Id == vehicleId);

                var days = (endDate - startDate).Days;
                var totalPrice = days * vehicle.DailyRate;

                return Json(new 
                { 
                    available = isAvailable, 
                    totalPrice = totalPrice, 
                    days = days 
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error during AJAX availability check");
                return Json(new { available = false, message = "System error checking availability." });
            }
        }
    }
}
