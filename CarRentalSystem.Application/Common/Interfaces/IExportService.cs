using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentalSystem.Application.Common.Interfaces
{
    public interface IExportService
    {
        Task<byte[]> ExportReservationsToExcelAsync(CancellationToken cancellationToken = default);
        Task<byte[]> ExportVehiclesToExcelAsync(CancellationToken cancellationToken = default);
        Task<byte[]> ExportPaymentsToExcelAsync(Guid? reservationId = null, CancellationToken cancellationToken = default);
    }
}
