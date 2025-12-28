using CarRentalSystem.FrontOffice.Helpers;
using CarRentalSystem.FrontOffice.Models.DTOs;
using CarRentalSystem.FrontOffice.Models.ViewModels;
using CarRentalSystem.FrontOffice.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.FrontOffice.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly ApiClient _apiClient;

        public AuthController(AuthService authService, ApiClient apiClient)
        {
            _authService = authService;
            _apiClient = apiClient;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (HttpContext.IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await _authService.LoginAsync(model.Email, model.Password);

            if (success)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);
                
                return RedirectToAction("Catalog", "Vehicles");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Additional validation
            var age = DateTime.Today.Year - model.DateOfBirth.Year;
            if (model.DateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
            
            if (age < 18)
            {
                ModelState.AddModelError("DateOfBirth", "You must be at least 18 years old to register.");
                return View(model);
            }

            var request = new RegisterCustomerRequest
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                LicenseNumber = model.LicenseNumber,
                DateOfBirth = model.DateOfBirth
            };

            var success = await _authService.RegisterAsync(request);

            if (success)
            {
                // Auto login after successful registration
                await _authService.LoginAsync(model.Email, model.Password);
                return RedirectToAction("Catalog", "Vehicles");
            }

            ModelState.AddModelError("", "Registration failed. Please try again.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var profile = await _apiClient.GetCustomerProfileAsync();
            if (profile == null)
            {
                return RedirectToAction("Login");
            }
            return View(profile);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(CustomerDetailsDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await _apiClient.UpdateCustomerProfileAsync(model);

            if (success)
            {
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            TempData["Error"] = "Failed to update profile. Please try again.";
            return View(model);
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
