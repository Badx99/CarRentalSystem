using CarRentalSystem.FrontOffice.Models.DTOs;

namespace CarRentalSystem.FrontOffice.Models.ViewModels
{
    public class MyReservationsViewModel
    {
        public List<ReservationDto> CurrentReservations { get; set; } = new();
        public List<ReservationDto> PastReservations { get; set; } = new();

        public void SortReservations(IEnumerable<ReservationDto> allReservations)
        {
            var activeStatuses = new[] { "Pending", "Confirmed", "InProgress" };
            var pastStatuses = new[] { "Completed", "Cancelled" };

            CurrentReservations = allReservations
                .Where(r => activeStatuses.Contains(r.Status))
                .OrderByDescending(r => r.StartDate)
                .ToList();

            PastReservations = allReservations
                .Where(r => pastStatuses.Contains(r.Status))
                .OrderByDescending(r => r.EndDate)
                .ToList();
        }
    }
}
