using MediatR;

namespace CarRentalSystem.Application.Features.Reservations.Commands.CreateReservation
{
    public record CreateReservationCommand : IRequest<CreateReservationResponse>
    {
        public Guid CustomerId { get; init; }
        public Guid VehicleId { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public string? Notes { get; init; }
    }

    public record CreateReservationResponse
    {
        public Guid Id { get; init; }
        public Guid CustomerId { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public Guid VehicleId { get; init; }
        public string VehicleInfo { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public int RentalDays { get; init; }
        public decimal TotalAmount { get; init; }
        public string Status { get; init; } = string.Empty;
        public string? QRCode { get; init; }
        public string Message { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}
