using CarRentalSystem.Application.Features.Payments.Queries.GetAllPayments;
using CarRentalSystem.Domain.Enums;
using CarRentalSystem.Domain.Interfaces;
using MediatR;

namespace CarRentalSystem.Application.Features.Payments.Queries.GetPaymentsByReservation
{
    public class GetPaymentsByReservationQueryHandler : IRequestHandler<GetPaymentsByReservationQuery, GetPaymentsByReservationResponse>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IReservationRepository _reservationRepository;

        public GetPaymentsByReservationQueryHandler(
            IPaymentRepository paymentRepository,
            IReservationRepository reservationRepository)
        {
            _paymentRepository = paymentRepository;
            _reservationRepository = reservationRepository;
        }

        public async Task<GetPaymentsByReservationResponse> Handle(
            GetPaymentsByReservationQuery request,
            CancellationToken cancellationToken)
        {
            var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId, cancellationToken);
            if (reservation == null)
            {
                throw new KeyNotFoundException($"Reservation with ID '{request.ReservationId}' not found.");
            }

            var payments = await _paymentRepository.GetByReservationIdAsync(request.ReservationId, cancellationToken);
            var paymentList = payments.ToList();

            var totalPaid = paymentList
                .Where(p => p.Status == PaymentStatus.Completed)
                .Sum(p => p.Amount);

            return new GetPaymentsByReservationResponse
            {
                ReservationId = request.ReservationId,
                TotalAmount = reservation.TotalAmount,
                TotalPaid = totalPaid,
                RemainingBalance = reservation.GetRemainingBalance(totalPaid),
                Payments = paymentList.Select(p => new PaymentDto
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    Method = p.Method.ToString(),
                    Status = p.Status.ToString(),
                    PaymentDate = p.PaymentDate,
                    TransactionReference = p.TransactionReference,
                    Notes = p.Notes,
                    CreatedAt = p.CreatedAt
                })
            };
        }
    }
}
