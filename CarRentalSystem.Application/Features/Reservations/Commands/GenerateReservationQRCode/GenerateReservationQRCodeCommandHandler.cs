using CarRentalSystem.Application.Common.Interfaces;
using CarRentalSystem.Domain.Interfaces;
using MediatR;
using MediatR.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarRentalSystem.Application.Features.Reservations.Commands.GenerateReservationQRCode
{
    public class GenerateReservationQRCodeCommandHandler : IRequestHandler<GenerateReservationQRCodeCommand, GenerateReservationQRCodeResponse>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IQRCodeService _qrCodeService;

        public GenerateReservationQRCodeCommandHandler(
            IReservationRepository reservationRepository,
            IQRCodeService qrCodeService)
        {
            _reservationRepository = reservationRepository;
            _qrCodeService = qrCodeService;
        }

        public async Task<GenerateReservationQRCodeResponse> Handle(
            GenerateReservationQRCodeCommand request,
            CancellationToken cancellationToken)
        {
            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(
                request.ReservationId,
                cancellationToken) 
                ?? throw new KeyNotFoundException($"Reservation with ID {request.ReservationId} not found");

            // Create QR code content with reservation details
            var qrContent = new
            {
                ReservationId = reservation.Id,
                CustomerName = reservation.Customer.FullName,
                CustomerEmail = reservation.Customer.Email,
                VehicleBrand = reservation.Vehicle.Brand,
                VehicleModel = reservation.Vehicle.Model,
                LicensePlate = reservation.Vehicle.LicensePlate,
                StartDate = reservation.StartDate.ToString("yyyy-MM-dd"),
                EndDate = reservation.EndDate.ToString("yyyy-MM-dd"),
                TotalAmount = reservation.TotalAmount,
                Status = reservation.Status.ToString()
            };

            var qrContentJson = JsonSerializer.Serialize(qrContent);
            var qrCodeBase64 = _qrCodeService.GenerateQRCode(qrContentJson);

            // Store QR code in reservation
            reservation.SetQRCode(qrCodeBase64);
            await _reservationRepository.UpdateAsync(reservation, cancellationToken);

            return new GenerateReservationQRCodeResponse
            {
                ReservationId = reservation.Id,
                QRCodeBase64 = qrCodeBase64,
                Message = "QR code generated successfully"
            };
        }
    }
}
