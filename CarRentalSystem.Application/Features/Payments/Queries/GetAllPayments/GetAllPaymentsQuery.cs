using MediatR;

namespace CarRentalSystem.Application.Features.Payments.Queries.GetAllPayments
{
    public record GetAllPaymentsQuery : IRequest<IEnumerable<PaymentDto>>;

    public record PaymentDto
    {
        public Guid Id { get; init; }
        public Guid ReservationId { get; init; }
        public decimal Amount { get; init; }
        public string Method { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime PaymentDate { get; init; }
        public string? TransactionReference { get; init; }
        public string? Notes { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
