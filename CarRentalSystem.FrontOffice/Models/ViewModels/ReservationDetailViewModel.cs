using CarRentalSystem.FrontOffice.Models.DTOs;

namespace CarRentalSystem.FrontOffice.Models.ViewModels
{
    public class ReservationDetailViewModel
    {
        public ReservationDetailDto Reservation { get; set; } = new();
        public bool CanCancel => Reservation.Status == "Pending" || Reservation.Status == "Confirmed";
        public bool CanDownloadReceipt => Reservation.Status == "Completed";
    }
}
