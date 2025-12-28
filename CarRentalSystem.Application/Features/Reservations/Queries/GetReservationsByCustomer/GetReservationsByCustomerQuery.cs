using MediatR;

namespace CarRentalSystem.Application.Features.Reservations.Queries.GetReservationsByCustomer
{
    public record GetReservationsByCustomerQuery : IRequest<GetReservationsByCustomerResponse>
    {
        public Guid CustomerId { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public record CustomerReservationDto
    {
        public Guid Id { get; init; }
        public Guid VehicleId { get; init; }
        public string VehicleBrand { get; init; } = string.Empty;
        public string VehicleModel { get; init; } = string.Empty;
        public string VehicleLicensePlate { get; init; } = string.Empty;
        public string VehicleTypeName { get; init; } = string.Empty;
        public string? VehicleImageUrl { get; init; }
        public string VehicleInfo { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public int RentalDays { get; init; }
        public decimal TotalAmount { get; init; }
        public string Status { get; init; } = string.Empty;
        public string? QRCode { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public record GetReservationsByCustomerResponse
    {
        public Guid CustomerId { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public IEnumerable<CustomerReservationDto> Reservations { get; init; } = Enumerable.Empty<CustomerReservationDto>();
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
    }
}
