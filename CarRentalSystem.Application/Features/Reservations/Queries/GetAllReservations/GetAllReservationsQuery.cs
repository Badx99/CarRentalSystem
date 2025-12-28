using MediatR;

namespace CarRentalSystem.Application.Features.Reservations.Queries.GetAllReservations
{
    public record GetAllReservationsQuery : IRequest<GetAllReservationsResponse>
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string? Status { get; init; }
    }

    public record ReservationDto
    {
        public Guid Id { get; init; }
        public Guid CustomerId { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public string CustomerEmail { get; init; } = string.Empty;
        public Guid VehicleId { get; init; }
        public string VehicleInfo { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public int RentalDays { get; init; }
        public decimal TotalAmount { get; init; }
        public string Status { get; init; } = string.Empty;
        public string PaymentStatus { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }

    public record GetAllReservationsResponse
    {
        public IEnumerable<ReservationDto> Reservations { get; init; } = Enumerable.Empty<ReservationDto>();
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
    }
}
