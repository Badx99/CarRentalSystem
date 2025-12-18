using MediatR;

namespace CarRentalSystem.Application.Features.Reservations.Queries.GenerateReservationReceipt;

public record GenerateReservationReceiptQuery(Guid ReservationId) : IRequest<ReservationReceiptDto>;

public record ReservationReceiptDto
{
    public byte[] PdfBytes { get; init; } = Array.Empty<byte>();
    public string FileName { get; init; } = string.Empty;
}