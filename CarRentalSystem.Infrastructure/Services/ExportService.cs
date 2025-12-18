using CarRentalSystem.Application.Common.Interfaces;
using CarRentalSystem.Domain.Enums;
using CarRentalSystem.Domain.Interfaces;
using ClosedXML.Excel;

namespace CarRentalSystem.Infrastructure.Services;

/// <summary>
/// Export service using ClosedXML
/// </summary>
public class ExportService : IExportService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IPaymentRepository _paymentRepository;

    public ExportService(
        IReservationRepository reservationRepository,
        IVehicleRepository vehicleRepository,
        IPaymentRepository paymentRepository)
    {
        _reservationRepository = reservationRepository;
        _vehicleRepository = vehicleRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<byte[]> ExportReservationsToExcelAsync(CancellationToken cancellationToken = default)
    {
        var reservations = await _reservationRepository.GetAllAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Reservations");

        // Headers
        worksheet.Cell(1, 1).Value = "Reservation ID";
        worksheet.Cell(1, 2).Value = "Customer Name";
        worksheet.Cell(1, 3).Value = "Customer Email";
        worksheet.Cell(1, 4).Value = "Vehicle";
        worksheet.Cell(1, 5).Value = "License Plate";
        worksheet.Cell(1, 6).Value = "Start Date";
        worksheet.Cell(1, 7).Value = "End Date";
        worksheet.Cell(1, 8).Value = "Total Amount";
        worksheet.Cell(1, 9).Value = "Status";
        worksheet.Cell(1, 10).Value = "Created At";

        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, 10);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Data
        int row = 2;
        foreach (var reservation in reservations)
        {
            worksheet.Cell(row, 1).Value = reservation.Id.ToString();
            worksheet.Cell(row, 2).Value = reservation.Customer.FullName;
            worksheet.Cell(row, 3).Value = reservation.Customer.Email;
            worksheet.Cell(row, 4).Value = $"{reservation.Vehicle.Brand} {reservation.Vehicle.Model}";
            worksheet.Cell(row, 5).Value = reservation.Vehicle.LicensePlate;
            worksheet.Cell(row, 6).Value = reservation.StartDate.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 7).Value = reservation.EndDate.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 8).Value = reservation.TotalAmount;
            worksheet.Cell(row, 8).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 9).Value = reservation.Status.ToString();
            worksheet.Cell(row, 10).Value = reservation.CreatedAt.ToString("yyyy-MM-dd HH:mm");
            row++;
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportVehiclesToExcelAsync(CancellationToken cancellationToken = default)
    {
        var vehicles = await _vehicleRepository.GetAllAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Vehicles");

        // Headers
        worksheet.Cell(1, 1).Value = "Vehicle ID";
        worksheet.Cell(1, 2).Value = "Brand";
        worksheet.Cell(1, 3).Value = "Model";
        worksheet.Cell(1, 4).Value = "Year";
        worksheet.Cell(1, 5).Value = "License Plate";
        worksheet.Cell(1, 6).Value = "Color";
        worksheet.Cell(1, 7).Value = "Mileage";
        worksheet.Cell(1, 8).Value = "Status";
        worksheet.Cell(1, 9).Value = "Vehicle Type";
        worksheet.Cell(1, 10).Value = "Daily Rate";

        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, 10);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Data
        int row = 2;
        foreach (var vehicle in vehicles)
        {
            worksheet.Cell(row, 1).Value = vehicle.Id.ToString();
            worksheet.Cell(row, 2).Value = vehicle.Brand;
            worksheet.Cell(row, 3).Value = vehicle.Model;
            worksheet.Cell(row, 4).Value = vehicle.Year;
            worksheet.Cell(row, 5).Value = vehicle.LicensePlate;
            worksheet.Cell(row, 6).Value = vehicle.Color;
            worksheet.Cell(row, 7).Value = vehicle.Mileage;
            worksheet.Cell(row, 8).Value = vehicle.Status.ToString();
            worksheet.Cell(row, 9).Value = vehicle.VehicleType.Name;
            worksheet.Cell(row, 10).Value = vehicle.DailyRate;
            worksheet.Cell(row, 10).Style.NumberFormat.Format = "$#,##0.00";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportPaymentsToExcelAsync(
        Guid? reservationId = null,
        CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Payments");

        // Headers
        worksheet.Cell(1, 1).Value = "Payment ID";
        worksheet.Cell(1, 2).Value = "Reservation ID";
        worksheet.Cell(1, 3).Value = "Amount";
        worksheet.Cell(1, 4).Value = "Method";
        worksheet.Cell(1, 5).Value = "Status";
        worksheet.Cell(1, 6).Value = "Payment Date";
        worksheet.Cell(1, 7).Value = "Transaction Reference";

        var headerRange = worksheet.Range(1, 1, 1, 7);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Get payments
        IEnumerable<Domain.Entities.Payment> payments;
        if (reservationId.HasValue)
        {
            payments = await _paymentRepository.GetByReservationIdAsync(reservationId.Value, cancellationToken);
        }
        else
        {
            // Get all payments (you'd need to add this method to repository)
            var allReservations = await _reservationRepository.GetAllAsync(cancellationToken);
            var paymentsList = new List<Domain.Entities.Payment>();
            foreach (var reservation in allReservations)
            {
                var resPayments = await _paymentRepository.GetByReservationIdAsync(reservation.Id, cancellationToken);
                paymentsList.AddRange(resPayments);
            }
            payments = paymentsList;
        }

        // Data
        int row = 2;
        foreach (var payment in payments)
        {
            worksheet.Cell(row, 1).Value = payment.Id.ToString();
            worksheet.Cell(row, 2).Value = payment.ReservationId.ToString();
            worksheet.Cell(row, 3).Value = payment.Amount;
            worksheet.Cell(row, 3).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 4).Value = payment.Method.ToString();
            worksheet.Cell(row, 5).Value = payment.Status.ToString();
            worksheet.Cell(row, 6).Value = payment.PaymentDate.ToString("yyyy-MM-dd HH:mm");
            worksheet.Cell(row, 7).Value = payment.TransactionReference ?? "";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}