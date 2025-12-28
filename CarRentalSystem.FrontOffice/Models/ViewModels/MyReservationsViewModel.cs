using CarRentalSystem.FrontOffice.Models.DTOs;

namespace CarRentalSystem.FrontOffice.Models.ViewModels
{
    public class MyReservationsViewModel
    {
        public List<ReservationDto> CurrentReservations { get; set; } = new();
        public List<ReservationDto> PastReservations { get; set; } = new();

        // Filters
        public string? StatusFilter { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public void SortReservations(IEnumerable<ReservationDto> allReservations)
        {
            var filtered = allReservations;

            // Apply filters
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                filtered = filtered.Where(r => r.Status.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (DateFrom.HasValue)
            {
                filtered = filtered.Where(r => r.StartDate >= DateFrom.Value);
            }

            if (DateTo.HasValue)
            {
                filtered = filtered.Where(r => r.EndDate <= DateTo.Value);
            }

            var activeStatuses = new[] { "Pending", "Confirmed", "InProgress" };
            var pastStatuses = new[] { "Completed", "Cancelled" };

            CurrentReservations = filtered
                .Where(r => activeStatuses.Contains(r.Status))
                .OrderByDescending(r => r.StartDate)
                .ToList();

            PastReservations = filtered
                .Where(r => pastStatuses.Contains(r.Status))
                .OrderByDescending(r => r.EndDate)
                .ToList();
        }
    }
}
