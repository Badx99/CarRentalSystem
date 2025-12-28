namespace CarRentalSystem.FrontOffice.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public Guid ReservationId { get; set; }
        public string VehicleInfo { get; set; } = string.Empty;
        public string VehicleImageUrl { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RentalDays { get; set; }
        public decimal TotalAmount { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        
        // Payment form fields
        public int PaymentMethod { get; set; } = 2; // Default to CreditCard
        public string? TransactionReference { get; set; }
        public string? Notes { get; set; }
    }
}
