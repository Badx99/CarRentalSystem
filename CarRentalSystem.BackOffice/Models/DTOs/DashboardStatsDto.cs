namespace CarRentalSystem.BackOffice.Models.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalVehicles { get; set; }
        public int AvailableVehicles { get; set; }
        public int RentedVehicles { get; set; }
        public int MaintenanceVehicles { get; set; }

        public int TotalReservations { get; set; }
        public int PendingReservations { get; set; }
        public int ConfirmedReservations { get; set; }
        public int ActiveRentals { get; set; }
        public int CompletedReservations { get; set; }

        public int TotalCustomers { get; set; }
        public int TotalEmployees { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal PendingPayments { get; set; }
    }
}
