using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentalSystem.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string customerName, CancellationToken cancellationToken = default);

        Task SendReservationConfirmationEmailAsync(
            string toEmail,
            string customerName,
            Guid reservationId,
            DateTime startDate,
            DateTime endDate,
            string vehicleInfo,
            CancellationToken cancellationToken = default);

        Task SendReservationCancellationEmailAsync(
            string toEmail,
            string customerName,
            Guid reservationId,
            CancellationToken cancellationToken = default);

        Task SendPaymentConfirmationEmailAsync(
            string toEmail,
            string customerName,
            decimal amount,
            Guid reservationId,
            CancellationToken cancellationToken = default);
    }
}
