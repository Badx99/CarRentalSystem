using CarRentalSystem.Application.Common.Interfaces;
using CarRentalSystem.Domain.Entities;
using CarRentalSystem.Domain.Interfaces;
using MediatR;

namespace CarRentalSystem.Application.Features.Reservations.Commands.CreateReservation
{
    public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, CreateReservationResponse>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IQRCodeService _qrCodeService;
        private readonly IEmailService _emailService;

        public CreateReservationCommandHandler(
            IReservationRepository reservationRepository,
            ICustomerRepository customerRepository,
            IVehicleRepository vehicleRepository,
            IQRCodeService qrCodeService,
            IEmailService emailService)
        {
            _reservationRepository = reservationRepository;
            _customerRepository = customerRepository;
            _vehicleRepository = vehicleRepository;
            _qrCodeService = qrCodeService;
            _emailService = emailService;
        }

        public async Task<CreateReservationResponse> Handle(
            CreateReservationCommand request,
            CancellationToken cancellationToken)
        {
            // Validate customer exists
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with ID '{request.CustomerId}' not found.");
            }

            // Check customer eligibility
            if (!customer.IsEligible())
            {
                throw new InvalidOperationException("Customer is not eligible to make reservations.");
            }

            // Validate vehicle exists and is available
            var vehicle = await _vehicleRepository.GetByIdWithTypeAsync(request.VehicleId, cancellationToken);
            if (vehicle == null)
            {
                throw new KeyNotFoundException($"Vehicle with ID '{request.VehicleId}' not found.");
            }

            // Check vehicle availability for the date range
            var isAvailable = await _vehicleRepository.IsAvailableAsync(
                request.VehicleId,
                request.StartDate,
                request.EndDate,
                cancellationToken);

            if (!isAvailable)
            {
                throw new InvalidOperationException("Vehicle is not available for the selected dates.");
            }

            // Get effective daily rate
            var dailyRate = vehicle.GetEffectiveDailyRate();

            // Create the reservation
            var reservation = Reservation.Create(
                customerId: request.CustomerId,
                vehicleId: request.VehicleId,
                startDate: request.StartDate,
                endDate: request.EndDate,
                dailyRate: dailyRate,
                notes: request.Notes
            );

            // Generate QR Code with reservation details
            var qrContent = reservation.Id.ToString();
            var qrCodeBase64 = _qrCodeService.GenerateQRCode(qrContent);
            reservation.SetQRCode(qrCodeBase64);

            await _reservationRepository.AddAsync(reservation, cancellationToken);

            // Send confirmation email
            var vehicleInfo = $"{vehicle.Brand} {vehicle.Model} ({vehicle.LicensePlate})";
            try
            {
                await _emailService.SendReservationConfirmationEmailAsync(
                    customer.Email,
                    customer.FullName,
                    reservation.Id,
                    reservation.StartDate,
                    reservation.EndDate,
                    vehicleInfo,
                    cancellationToken);
            }
            catch (Exception)
            {
                // Log but don't fail the reservation if email fails
            }

            return new CreateReservationResponse
            {
                Id = reservation.Id,
                CustomerId = reservation.CustomerId,
                CustomerName = customer.FullName,
                VehicleId = reservation.VehicleId,
                VehicleInfo = vehicleInfo,
                StartDate = reservation.StartDate,
                EndDate = reservation.EndDate,
                RentalDays = reservation.GetRentalDays(),
                TotalAmount = reservation.TotalAmount,
                Status = reservation.Status.ToString(),
                QRCode = qrCodeBase64,
                Message = "Reservation created successfully. Awaiting confirmation.",
                CreatedAt = reservation.CreatedAt
            };
        }
    }
}

