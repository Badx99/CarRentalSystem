using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentalSystem.Application.Common.Interfaces
{
    public interface IPdfService
    {
        
        // Generates a reservation receipt as PDF
        Task<byte[]> GenerateReservationReceiptAsync(Guid reservationId, CancellationToken cancellationToken = default);
    }
}
