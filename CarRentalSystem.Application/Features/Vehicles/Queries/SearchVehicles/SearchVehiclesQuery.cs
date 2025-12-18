using CarRentalSystem.Application.Common.Models;
using CarRentalSystem.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentalSystem.Application.Features.Vehicles.Queries.SearchVehicles
{
    public record SearchVehiclesQuery : IRequest<PagedResult<VehicleSearchDto>>
    {
        public string? SearchTerm { get; init; }
        public VehicleStatus? Status { get; init; }
        public Guid? VehicleTypeId { get; init; }
        public int? MinYear { get; init; }
        public int? MaxYear { get; init; }
        public decimal? MinDailyRate { get; init; }
        public decimal? MaxDailyRate { get; init; }
        public string? SortBy { get; init; } // Brand, Model, Year, DailyRate
        public bool SortDescending { get; init; } = false;
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public record VehicleSearchDto
    {
        public Guid Id { get; init; }
        public string Brand { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public int Year { get; init; }
        public string LicensePlate { get; init; } = string.Empty;
        public string Color { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string VehicleTypeName { get; init; } = string.Empty;
        public decimal DailyRate { get; init; }
        public string? ImageUrl { get; init; }
    }
}
