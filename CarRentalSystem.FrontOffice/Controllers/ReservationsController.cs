using CarRentalSystem.FrontOffice.Helpers;
using CarRentalSystem.FrontOffice.Models.DTOs;
using CarRentalSystem.FrontOffice.Models.ViewModels;
using CarRentalSystem.FrontOffice.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.FrontOffice.Controllers
{
    
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly ApiClient _apiClient;

        public ReservationsController(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> MyReservations(string? status, DateTime? dateFrom, DateTime? dateTo)
        {
            var reservations = await _apiClient.GetMyReservationsAsync();
            var viewModel = new MyReservationsViewModel
            {
                StatusFilter = status,
                DateFrom = dateFrom,
                DateTo = dateTo
            };
            viewModel.SortReservations(reservations);
            return View(viewModel);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var reservation = await _apiClient.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            // Ensure reservation belongs to current user
            var currentUserId = HttpContext.GetCurrentUserId();
            if (reservation.CustomerId != currentUserId)
            {
                return Forbid();
            }

            var viewModel = new ReservationDetailViewModel
            {
                Reservation = reservation
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Book(Guid vehicleId, DateTime startDate, DateTime endDate)
        {
            var vehicleResponse = await _apiClient.GetVehicleByIdAsync(vehicleId);
            if (vehicleResponse == null)
            {
                return NotFound();
            }

            var vehicleDto = new VehicleDto
            {
                Id = vehicleResponse.Id,
                Brand = vehicleResponse.Brand,
                Model = vehicleResponse.Model,
                Year = vehicleResponse.Year,
                LicensePlate = vehicleResponse.LicensePlate,
                Color = vehicleResponse.Color,
                VehicleTypeName = vehicleResponse.VehicleTypeName,
                DailyRate = vehicleResponse.EffectiveDailyRate,
                ImageUrl = vehicleResponse.ImageUrl,
                PassengerCapacity = vehicleResponse.PassengerCapacity,
                Mileage = vehicleResponse.Mileage
            };

            // Verify basic validity
            if (startDate < DateTime.Today || endDate <= startDate)
            {
                TempData["Error"] = "Invalid dates selected.";
                return RedirectToAction("Details", "Vehicles", new { id = vehicleId });
            }

            // Double check availability before showing booking form
            var availableVehicles = await _apiClient.GetAvailableVehiclesAsync(startDate, endDate);
            if (!availableVehicles.Any(v => v.Id == vehicleId))
            {
                TempData["Error"] = "This vehicle is no longer available for the selected dates.";
                return RedirectToAction("Details", "Vehicles", new { id = vehicleId });
            }

            var viewModel = new BookingViewModel
            {
                Vehicle = vehicleDto,
                StartDate = startDate,
                EndDate = endDate
            };
            viewModel.Calculate();

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookingViewModel model)
        {
            // Re-fetch vehicle to ensure data integrity (price, etc.)
            var vehicleResponse = await _apiClient.GetVehicleByIdAsync(model.Vehicle.Id);
            if (vehicleResponse == null)
            {
                TempData["Error"] = "Vehicle not found.";
                return RedirectToAction("Catalog", "Vehicles");
            }

            var vehicleDto = new VehicleDto
            {
                Id = vehicleResponse.Id,
                Brand = vehicleResponse.Brand,
                Model = vehicleResponse.Model,
                Year = vehicleResponse.Year,
                LicensePlate = vehicleResponse.LicensePlate,
                Color = vehicleResponse.Color,
                VehicleTypeName = vehicleResponse.VehicleTypeName,
                DailyRate = vehicleResponse.EffectiveDailyRate,
                ImageUrl = vehicleResponse.ImageUrl,
                PassengerCapacity = vehicleResponse.PassengerCapacity,
                Mileage = vehicleResponse.Mileage,
                
            };
            model.Vehicle = vehicleDto; // Restore vehicle data for View return case

            if (!ModelState.IsValid)
                return View(model);

            var userId = HttpContext.GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Book", new { vehicleId = model.Vehicle.Id, startDate = model.StartDate, endDate = model.EndDate }) });
            }

            var request = new CreateReservationRequest
            {
                CustomerId = userId.Value,
                VehicleId = model.Vehicle.Id,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Notes = model.Notes
            };

            var response = await _apiClient.CreateReservationAsync(request);

            if (response != null)
            {
                // Redirect to Checkout for payment
                return RedirectToAction("Checkout", new { id = response.Id });
            }

            ModelState.AddModelError("", "Failed to create reservation. The vehicle might be booked or an error occurred.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(Guid id)
        {
            var reservation = await _apiClient.GetReservationByIdAsync(id);
            if (reservation == null)
                return NotFound();

            var currentUserId = HttpContext.GetCurrentUserId();
            if (reservation.CustomerId != currentUserId)
                return Forbid();

            // Only allow checkout for Pending reservations
            if (reservation.Status != "Pending")
            {
                TempData["Error"] = "This reservation is already processed.";
                return RedirectToAction("Details", new { id });
            }

            var viewModel = new CheckoutViewModel
            {
                ReservationId = reservation.Id,
                VehicleInfo = $"{reservation.VehicleBrand} {reservation.VehicleModel}",
                VehicleImageUrl = reservation.VehicleImageUrl ?? "",
                StartDate = reservation.StartDate,
                EndDate = reservation.EndDate,
                RentalDays = (int)(reservation.EndDate - reservation.StartDate).TotalDays,
                TotalAmount = reservation.TotalAmount,
                CustomerName = reservation.CustomerName ?? ""
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(CheckoutViewModel model)
        {
            var reservation = await _apiClient.GetReservationByIdAsync(model.ReservationId);
            if (reservation == null)
                return NotFound();

            var currentUserId = HttpContext.GetCurrentUserId();
            if (reservation.CustomerId != currentUserId)
                return Forbid();

            // Create payment
            var paymentRequest = new CreatePaymentRequest
            {
                ReservationId = model.ReservationId,
                Amount = model.TotalAmount,
                Method = model.PaymentMethod,
                TransactionReference = model.TransactionReference,
                Notes = model.Notes
            };

            var paymentResponse = await _apiClient.CreatePaymentAsync(paymentRequest);
            if (paymentResponse == null)
            {
                TempData["Error"] = "Payment processing failed. Please try again.";
                return RedirectToAction("Checkout", new { id = model.ReservationId });
            }

            // Payment successful - redirect to confirmation
            TempData["Success"] = "Payment successful! Your booking is confirmed.";
            return RedirectToAction("Confirmation", new { id = model.ReservationId });
        }

        public async Task<IActionResult> Confirmation(Guid id)
        {
            var reservation = await _apiClient.GetReservationByIdAsync(id);
            if (reservation == null)
                return NotFound();

            var currentUserId = HttpContext.GetCurrentUserId();
            if (reservation.CustomerId != currentUserId)
                return Forbid();

            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id)
        {
            // Basic ownership check via GetReservationById before action
            var reservation = await _apiClient.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();
            
            var currentUserId = HttpContext.GetCurrentUserId();
            if (reservation.CustomerId != currentUserId) return Forbid();

            var success = await _apiClient.CancelReservationAsync(id);
            if (success)
            {
                TempData["Success"] = "Reservation cancelled successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to cancel reservation. It may not be in a cancelable state.";
            }

            return RedirectToAction("MyReservations");
        }

        public async Task<IActionResult> DownloadReceipt(Guid id)
        {
            // API client receipt implementation will handle the call
            // We should verify ownership first though if possible
            var reservation = await _apiClient.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();

            var currentUserId = HttpContext.GetCurrentUserId();
            if (reservation.CustomerId != currentUserId) return Forbid();

            var pdfBytes = await _apiClient.DownloadReceiptAsync(id);
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                TempData["Error"] = "Receipt not available.";
                return RedirectToAction("Details", new { id });
            }

            return File(pdfBytes, "application/pdf", $"Reservation_{id}.pdf");
        }
    }
}

