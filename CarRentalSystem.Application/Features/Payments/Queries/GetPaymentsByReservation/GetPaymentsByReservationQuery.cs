using CarRentalSystem.Application.Features.Payments.Queries.GetAllPayments;
using MediatR;

namespace CarRentalSystem.Application.Features.Payments.Queries.GetPaymentsByReservation
{
    public record GetPaymentsByReservationQuery(Guid ReservationId) : IRequest<GetPaymentsByReservationResponse>;

    public record GetPaymentsByReservationResponse
    {
        public Guid ReservationId { get; init; }
        public decimal TotalAmount { get; init; }
        public decimal TotalPaid { get; init; }
        public decimal RemainingBalance { get; init; }
        public IEnumerable<PaymentDto> Payments { get; init; } = Enumerable.Empty<PaymentDto>();
    }
}
