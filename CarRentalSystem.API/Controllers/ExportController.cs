using CarRentalSystem.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator,Employee")]
    public class ExportsController : ControllerBase
    {
        private readonly IExportService _exportService;

        public ExportsController(IExportService exportService)
        {
            _exportService = exportService;
        }

        /// <summary>
        /// Export all reservations to Excel
        /// </summary>
        [HttpGet("reservations")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportReservations()
        {
            var excelBytes = await _exportService.ExportReservationsToExcelAsync();
            var fileName = $"Reservations_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Export all vehicles to Excel
        /// </summary>
        [HttpGet("vehicles")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportVehicles()
        {
            var excelBytes = await _exportService.ExportVehiclesToExcelAsync();
            var fileName = $"Vehicles_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Export payments to Excel
        /// </summary>
        [HttpGet("payments")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportPayments([FromQuery] Guid? reservationId = null)
        {
            var excelBytes = await _exportService.ExportPaymentsToExcelAsync(reservationId);
            var fileName = $"Payments_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
