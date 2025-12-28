using CarRentalSystem.Application.Common.Interfaces;
using CarRentalSystem.Domain.Enums;
using CarRentalSystem.Domain.Interfaces;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Image;
using iText.IO.Font.Constants;
using iText.Kernel.Font;


namespace CarRentalSystem.Infrastructure.Services
{
    public class PdfService : IPdfService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IPaymentRepository _paymentRepository;

        public PdfService(
            IReservationRepository reservationRepository,
            IPaymentRepository paymentRepository)
        {
            _reservationRepository = reservationRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<byte[]> GenerateReservationReceiptAsync(
            Guid reservationId,
            CancellationToken cancellationToken = default)
        {
            var reservation = await _reservationRepository.GetByIdWithDetailsAsync(
                reservationId,
                cancellationToken);

            if (reservation == null)
                throw new KeyNotFoundException($"Reservation with ID {reservationId} not found");

            var payments = await _paymentRepository.GetByReservationIdAsync(
                reservationId,
                cancellationToken);

            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Title
            var title = new Paragraph("CAR RENTAL RESERVATION RECEIPT")
                .SetFont(boldFont)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20)
                .SetMarginBottom(20);
            document.Add(title);

            // Company Info
            document.Add(new Paragraph("Car Rental System")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(12));
            document.Add(new Paragraph("Email: info@carrental.com | Phone: +1-234-567-8900")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10)
                .SetMarginBottom(30));

            // Reservation Details
            document.Add(new Paragraph("Reservation Details")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginBottom(10));

            var detailsTable = new Table(2).UseAllAvailableWidth();
            detailsTable.SetMarginBottom(20);

            AddTableRow(detailsTable, "Reservation ID:", reservation.Id.ToString());
            AddTableRow(detailsTable, "Status:", reservation.Status.ToString());
            AddTableRow(detailsTable, "Booking Date:", reservation.CreatedAt.ToString("yyyy-MM-dd HH:mm"));

            document.Add(detailsTable);

            // Customer Information
            document.Add(new Paragraph("Customer Information")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginBottom(10));

            var customerTable = new Table(2).UseAllAvailableWidth();
            customerTable.SetMarginBottom(20);

            AddTableRow(customerTable, "Name:", reservation.Customer.FullName);
            AddTableRow(customerTable, "Email:", reservation.Customer.Email);

            var customer = reservation.Customer as Domain.Entities.Customer;
            if (customer != null)
            {
                AddTableRow(customerTable, "Phone:", customer.PhoneNumber);
                AddTableRow(customerTable, "License Number:", customer.LicenseNumber);
            }

            document.Add(customerTable);

            // Vehicle Information
            document.Add(new Paragraph("Vehicle Information")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginBottom(10));

            var vehicleTable = new Table(2).UseAllAvailableWidth();
            vehicleTable.SetMarginBottom(20);

            AddTableRow(vehicleTable, "Vehicle:", $"{reservation.Vehicle.Brand} {reservation.Vehicle.Model}");
            AddTableRow(vehicleTable, "Year:", reservation.Vehicle.Year.ToString());
            AddTableRow(vehicleTable, "Color:", reservation.Vehicle.Color);
            AddTableRow(vehicleTable, "License Plate:", reservation.Vehicle.LicensePlate);
            AddTableRow(vehicleTable, "Vehicle Type:", reservation.Vehicle.VehicleType.Name);

            document.Add(vehicleTable);

            // Rental Period
            document.Add(new Paragraph("Rental Period")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginBottom(10));

            var periodTable = new Table(2).UseAllAvailableWidth();
            periodTable.SetMarginBottom(20);

            AddTableRow(periodTable, "Start Date:", reservation.StartDate.ToString("yyyy-MM-dd"));
            AddTableRow(periodTable, "End Date:", reservation.EndDate.ToString("yyyy-MM-dd"));
            AddTableRow(periodTable, "Number of Days:", reservation.GetRentalDays().ToString());
            AddTableRow(periodTable, "Daily Rate:", $"${reservation.Vehicle.DailyRate:F2}");

            document.Add(periodTable);

            // Payment Information
            document.Add(new Paragraph("Payment Information")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginBottom(10));

            var paymentTable = new Table(2).UseAllAvailableWidth();
            paymentTable.SetMarginBottom(20);

            var totalPaid = payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
            var remainingBalance = reservation.TotalAmount - totalPaid;

            AddTableRow(paymentTable, "Total Amount:", $"${reservation.TotalAmount:F2}");
            AddTableRow(paymentTable, "Amount Paid:", $"${totalPaid:F2}");
            AddTableRow(paymentTable, "Remaining Balance:", $"${remainingBalance:F2}");
            AddTableRow(paymentTable, "Payment Status:", reservation.IsFullyPaid() ? "Fully Paid" : "Pending");

            document.Add(paymentTable);

            // Payment History
            if (payments.Any())
            {
                document.Add(new Paragraph("Payment History")
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10));

                var historyTable = new Table(4).UseAllAvailableWidth();
                historyTable.SetMarginBottom(20);

                // Header
                historyTable.AddHeaderCell(new Cell().Add(new Paragraph("Date")
                    .SetFont(boldFont)));
                historyTable.AddHeaderCell(new Cell().Add(new Paragraph("Amount")
                    .SetFont(boldFont)));
                historyTable.AddHeaderCell(new Cell().Add(new Paragraph("Method")
                    .SetFont(boldFont)));
                historyTable.AddHeaderCell(new Cell().Add(new Paragraph("Status")
                    .SetFont(boldFont)));

                foreach (var payment in payments.OrderBy(p => p.PaymentDate))
                {
                    historyTable.AddCell(payment.PaymentDate.ToString("yyyy-MM-dd"));
                    historyTable.AddCell($"${payment.Amount:F2}");
                    historyTable.AddCell(payment.Method.ToString());
                    historyTable.AddCell(payment.Status.ToString());
                }

                document.Add(historyTable);
            }

            // QR Code (if exists)
            if (!string.IsNullOrEmpty(reservation.QRCode))
            {
                try
                {
                    var qrCodeBytes = Convert.FromBase64String(reservation.QRCode);
                    var qrCodeImage = new Image(ImageDataFactory.Create(qrCodeBytes));
                    qrCodeImage.SetWidth(150);
                    qrCodeImage.SetHorizontalAlignment(HorizontalAlignment.CENTER);

                    document.Add(new Paragraph("Scan QR Code for Quick Access")
                        .SetFont(boldFont)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(12)
                        .SetMarginTop(20)
                        .SetMarginBottom(10));

                    document.Add(qrCodeImage);
                }
                catch
                {
                    // Ignore QR code errors
                }
            }

            // Notes
            if (!string.IsNullOrEmpty(reservation.Notes))
            {
                document.Add(new Paragraph("Additional Notes")
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetMarginTop(20)
                    .SetMarginBottom(10));
                document.Add(new Paragraph(reservation.Notes)
                    .SetFontSize(10));
            }

            // Footer
            document.Add(new Paragraph("\nThank you for choosing our Car Rental Service!")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10)
                .SetMarginTop(30));

            document.Add(new Paragraph("For any inquiries, please contact us at info@carrental.com")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(8)
                .SetFontColor(ColorConstants.GRAY));

            document.Close();

            return memoryStream.ToArray();
        }

        private static void AddTableRow(Table table, string label, string value)
        {
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            
            table.AddCell(new Cell().Add(new Paragraph(label).SetFont(boldFont)));
            table.AddCell(new Cell().Add(new Paragraph(value)));
        }
    }
}
