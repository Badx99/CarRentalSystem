using CarRentalSystem.FrontOffice.Models;
using CarRentalSystem.FrontOffice.Models.ViewModels;
using CarRentalSystem.FrontOffice.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CarRentalSystem.FrontOffice.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApiClient apiClient, ILogger<HomeController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            try
            {
                var allVehicles = await _apiClient.GetAllVehiclesAsync();
                viewModel.FeaturedVehicles = allVehicles.Take(6).ToList();
                viewModel.TotalVehicles = allVehicles.Count;

                var types = await _apiClient.GetVehicleTypesAsync();
                viewModel.VehicleTypes = types.Select(t => t.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch home page data");
                TempData["Error"] = "We are currently experiencing technical difficulties. Some content may not load correctly.";
            }

            return View(viewModel);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var errorModel = new ErrorViewModel 
            { 
                RequestId = requestId,
                StatusCode = statusCode
            };

            if (statusCode.HasValue)
            {
                if (statusCode == 404)
                {
                    return View("NotFound");
                }

                if (statusCode == 401)
                {
                     // Should be intercepted by auth middleware/cookie options usually, but specific error page just in case
                     return RedirectToAction("Login", "Auth");
                }
            }
            
            // Get exception details if available
            var exceptionHandlerPathFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature?.Error != null)
            {
                errorModel.Message = "An unexpected error occurred on the server.";
                errorModel.Details = exceptionHandlerPathFeature.Error.ToString(); // Only show in Dev or managed via view logic
                _logger.LogError(exceptionHandlerPathFeature.Error, "Unhandled exception encountered");
            }

            return View(errorModel);
        }
    }
}
