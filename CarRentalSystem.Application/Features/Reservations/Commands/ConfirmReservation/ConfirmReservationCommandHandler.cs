using CarRentalSystem.Application.Common.Interfaces;
using CarRentalSystem.Domain.Interfaces;
using MediatR;

namespace CarRentalSystem.Application.Features.Reservations.Commands.ConfirmReservation
{
    public class ConfirmReservationCommandHandler : IRequestHandler<ConfirmReservationCommand, ConfirmReservationResponse>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IEmailService _emailService;

        public ConfirmReservationCommandHandler(IReservationRepository reservationRepository,IEmailService emailService)
        {
            _reservationRepository = reservationRepository;
            _emailService = emailService;
        }

        public async Task<ConfirmReservationResponse> Handle(
            ConfirmReservationCommand request,
            CancellationToken cancellationToken)
        {
            var reservation = await _reservationRepository.GetByIdAsync(request.Id, cancellationToken);
            if (reservation == null)
            {
                throw new KeyNotFoundException($"Reservation with ID '{request.Id}' not found.");
            }

            reservation.Confirm();
            await _reservationRepository.UpdateAsync(reservation, cancellationToken);

            //SEND CONFIRMATION EMAIL
            _ = Task.Run(async () =>
            {
                try
                {
                    var vehicleInfo = $"{reservation.Vehicle.Brand} {reservation.Vehicle.Model}";
                    await _emailService.SendReservationConfirmationEmailAsync(
                        reservation.Customer.Email,
                        reservation.Customer.FullName,
                        reservation.Id,
                        reservation.StartDate,
                        reservation.EndDate,
                        vehicleInfo,
                        cancellationToken);
                }
                catch
                {
                    // Ignore email errors
                }
            }, cancellationToken);


            return new ConfirmReservationResponse
            {
                Id = reservation.Id,
                Status = reservation.Status.ToString(),
                Message = "Reservation confirmed successfully.",
                UpdatedAt = reservation.UpdatedAt
            };
        }
    }
}
