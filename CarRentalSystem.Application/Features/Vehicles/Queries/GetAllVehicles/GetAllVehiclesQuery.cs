using MediatR;

namespace CarRentalSystem.Application.Features.Vehicles.Queries.GetAllVehicles
{
    public record GetAllVehiclesQuery : IRequest<GetAllVehiclesResponse>
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string? Status { get; init; }
        public Guid? VehicleTypeId { get; init; }
    }

    public record VehicleDto
    {
        public Guid Id { get; init; }
        public string Brand { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public int Year { get; init; }
        public string LicensePlate { get; init; } = string.Empty;
        public string Color { get; init; } = string.Empty;
        public int Mileage { get; init; }
        public string Status { get; init; } = string.Empty;
        public decimal? DailyRate { get; init; }
        public decimal EffectiveDailyRate { get; init; }
        public string? ImageUrl { get; init; }
        public Guid VehicleTypeId { get; init; }
        public string VehicleTypeName { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }

    public record GetAllVehiclesResponse
    {
        public IEnumerable<VehicleDto> Vehicles { get; init; } = Enumerable.Empty<VehicleDto>();
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
    }
}
