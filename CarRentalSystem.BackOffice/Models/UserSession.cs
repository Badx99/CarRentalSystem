namespace CarRentalSystem.BackOffice.Models
{
    public class UserSession
    {
        public string Token { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }

        public bool IsAuthenticated => !string.IsNullOrEmpty(Token) && ExpiresAt > DateTime.UtcNow;
        public bool IsAdministrator => UserType == "Administrator";
        public bool IsEmployee => UserType == "Employee";
    }
}
