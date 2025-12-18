using CarRentalSystem.Application.Common.Interfaces;
using MediatR;

namespace CarRentalSystem.Application.Features.Reservations.Queries.GenerateReservationReceipt;

public class GenerateReservationReceiptQueryHandler
    : IRequestHandler<GenerateReservationReceiptQuery, ReservationReceiptDto>
{
    private readonly IPdfService _pdfService;

    public GenerateReservationReceiptQueryHandler(IPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    public async Task<ReservationReceiptDto> Handle(
        GenerateReservationReceiptQuery request,
        CancellationToken cancellationToken)
    {
        var pdfBytes = await _pdfService.GenerateReservationReceiptAsync(
            request.ReservationId,
            cancellationToken);

        var fileName = $"Reservation_{request.ReservationId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

        return new ReservationReceiptDto
        {
            PdfBytes = pdfBytes,
            FileName = fileName
        };
    }
}