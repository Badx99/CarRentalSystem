using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentalSystem.Application.Features.Reservations.Commands.GenerateReservationQRCode
{
    public record GenerateReservationQRCodeCommand(Guid ReservationId) : IRequest<GenerateReservationQRCodeResponse>;

    public record GenerateReservationQRCodeResponse
    {
        public Guid ReservationId { get; init; }
        public string QRCodeBase64 { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
    }
}
