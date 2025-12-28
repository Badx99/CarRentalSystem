using CarRentalSystem.Domain.Entities;
using CarRentalSystem.Domain.Interfaces;
using MediatR;

namespace CarRentalSystem.Application.Features.Payments.Commands.CreatePayment
{
    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, CreatePaymentResponse>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IReservationRepository _reservationRepository;

        public CreatePaymentCommandHandler(
            IPaymentRepository paymentRepository,
            IReservationRepository reservationRepository)
        {
            _paymentRepository = paymentRepository;
            _reservationRepository = reservationRepository;
        }

        public async Task<CreatePaymentResponse> Handle(
            CreatePaymentCommand request,
            CancellationToken cancellationToken)
        {
            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(request.ReservationId, cancellationToken);
            if (reservation == null)
            {
                throw new KeyNotFoundException($"Reservation with ID '{request.ReservationId}' not found.");
            }

            //VERIFY AMOUNT DOESN'T EXCEED REMAINING BALANCE
            var remainingBalance = reservation.GetRemainingBalance();
            if (request.Amount > remainingBalance)
            {
                throw new ArgumentException($"Payment amount '{request.Amount}' exceeds the remaining balance '{remainingBalance}'.");
            }

            var payment = Payment.Create(
                reservationId: request.ReservationId,
                amount: request.Amount,
                method: request.Method,
                transactionReference: request.TransactionReference,
                notes: request.Notes
            );

            await _paymentRepository.AddAsync(payment, cancellationToken);

            //UPDATE REMAINING BALANCE
            var updatedRemainingBalance = remainingBalance - payment.Amount;

            // Confirm reservation if fully paid
            if (updatedRemainingBalance <= 0 && reservation.Status == Domain.Enums.ReservationStatus.Pending)
            {
                reservation.Confirm();
                await _reservationRepository.UpdateAsync(reservation, cancellationToken);
            }

            return new CreatePaymentResponse
            {
                Id = payment.Id,
                ReservationId = payment.ReservationId,
                Amount = payment.Amount,
                Method = payment.Method.ToString(),
                Status = payment.Status.ToString(),
                PaymentDate = payment.PaymentDate,
                RemainingBalance = updatedRemainingBalance
            };
        }
    }
}
