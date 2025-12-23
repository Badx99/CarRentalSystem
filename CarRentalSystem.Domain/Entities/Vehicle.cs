using CarRentalSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentalSystem.Domain.Entities
{
    public class Vehicle
    {
        public Guid Id { get; private set; }
        public string Brand { get; private set; } = string.Empty;
        public string Model { get; private set; } = string.Empty;
        public int Year { get; private set; }
        public string LicensePlate { get; private set; } = string.Empty;
        public string Color { get; private set; } = string.Empty;
        public int Mileage { get; private set; }
        public VehicleStatus Status { get; private set; }
        public string? ImageUrl { get; private set; }
        public decimal? DailyRate { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public Guid VehicleTypeId { get; private set; } // FOREIGN KEY

        // Navigation properties
        public VehicleType VehicleType { get; private set; } = null!;
        public ICollection<Reservation> Reservations { get; private set; } = new List<Reservation>();

        private Vehicle() { }

        public static Vehicle Create(
            string brand,
            string model,
            int year,
            string licensePlate,
            string color,
            int mileage,
            Guid vehicleTypeId,
            decimal? dailyRate = null,
            string? imageUrl = null)
        {
            ValidateVehicleInfo(brand, model, year, color, mileage, dailyRate);
            ValidateLicensePlate(licensePlate);

            return new Vehicle
            {
                Id = Guid.NewGuid(),
                Brand = brand.Trim(),
                Model = model.Trim(),
                Year = year,
                LicensePlate = licensePlate.Trim().ToUpper(),
                Color = color.Trim(),
                Mileage = mileage,
                Status = VehicleStatus.Available,
                DailyRate = dailyRate,
                VehicleTypeId = vehicleTypeId,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Update(
            string brand,
            string model,
            int year,
            string color,
            int mileage,
            Guid vehicleTypeId,
            decimal? dailyRate = null,
            string? imageUrl = null)
        {
            ValidateVehicleInfo(brand, model, year, color, mileage, dailyRate);

            Brand = brand.Trim();
            Model = model.Trim();
            Year = year;
            Color = color.Trim();
            Mileage = mileage;
            DailyRate = dailyRate;
            VehicleTypeId = vehicleTypeId;
            ImageUrl = imageUrl;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateMileage(int newMileage)
        {
            if (newMileage < Mileage)
                throw new ArgumentException("New mileage cannot be less than current mileage");

            Mileage = newMileage;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetStatus(VehicleStatus status)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsRented()
        {
            if (Status != VehicleStatus.Available)
                throw new InvalidOperationException("Vehicle must be available to be rented");

            Status = VehicleStatus.Rented;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsAvailable()
        {
            Status = VehicleStatus.Available;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SendToMaintenance()
        {
            Status = VehicleStatus.Maintenance;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsAvailableForRental() => Status == VehicleStatus.Available;

        public decimal GetEffectiveDailyRate()
        {
            // Use vehicle-specific rate if set, otherwise fall back to VehicleType base rate
            if (DailyRate.HasValue && DailyRate.Value > 0)
                return DailyRate.Value;
            if (VehicleType == null)
                throw new InvalidOperationException("VehicleType must be loaded to calculate the rate.");
            return VehicleType.BaseDailyRate;
        }


        // VALIDATION METHODS
        private static void ValidateVehicleInfo(
            string brand,
            string model,
            int year,
            string color,
            int mileage,
            decimal? dailyRate // Optional: if null, uses VehicleType.BaseDailyRate
        )
        {
            // Brand validation
            if (string.IsNullOrWhiteSpace(brand))
                throw new ArgumentException("Brand cannot be empty", nameof(brand));
            if (brand.Length > 100)
                throw new ArgumentException("Brand cannot exceed 100 characters", nameof(brand));

            // Model validation
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be empty", nameof(model));
            if (model.Length > 100)
                throw new ArgumentException("Model cannot exceed 100 characters", nameof(model));

            // Year validation
            var currentYear = DateTime.UtcNow.Year;
            if (year < 1900 || year > currentYear + 1)
                throw new ArgumentException($"Year must be between 1900 and {currentYear + 1}", nameof(year));
                       
            // Color validation
            if (string.IsNullOrWhiteSpace(color))
                throw new ArgumentException("Color cannot be empty", nameof(color));
            if (color.Length > 50)
                throw new ArgumentException("Color cannot exceed 50 characters", nameof(color));

            // Mileage validation
            if (mileage < 0)
                throw new ArgumentException("Mileage cannot be negative", nameof(mileage));
            if (mileage > 1000000)
                throw new ArgumentException("Mileage seems unreasonably high", nameof(mileage));

            // Rate validation: if provided, must be positive; null means use VehicleType base rate
            if (dailyRate.HasValue && dailyRate.Value < 0)
                throw new ArgumentException("Rate cannot be negative", nameof(dailyRate));
        }

        private static void ValidateLicensePlate(string licensePlate)
        {
            if (string.IsNullOrWhiteSpace(licensePlate))
                throw new ArgumentException("License plate cannot be empty", nameof(licensePlate));

            if (licensePlate.Length > 20)
                throw new ArgumentException("License plate cannot exceed 20 characters", nameof(licensePlate));
        }

    }
}
