namespace CarRentalSystem.FrontOffice.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string? Message { get; set; }
        public string? Details { get; set; }
        public int? StatusCode { get; set; }
    }
}
