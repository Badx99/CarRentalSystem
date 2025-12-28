namespace CarRentalSystem.BackOffice.Models.DTOs
{
    public class DashboardStatsDto
    {
        // Customer stats
        public int TotalCustomers { get; set; }

        // Vehicle stats
        public int TotalVehicles { get; set; }
        public int AvailableVehicles { get; set; }
        public int RentedVehicles { get; set; }
        public int VehiclesInMaintenance { get; set; }

        // Reservation stats
        public int TotalReservations { get; set; }
        public int ActiveReservations { get; set; }
        public int PendingReservations { get; set; }
        public int CompletedReservationsThisMonth { get; set; }
        public int ConfirmedReservationsThisMonth { get; set; }
        public int CancelledReservationsThisMonth { get; set; }

        // Payment stats
        public decimal TotalRevenueThisMonth { get; set; }
        public decimal PendingPayments { get; set; }
    }
}

