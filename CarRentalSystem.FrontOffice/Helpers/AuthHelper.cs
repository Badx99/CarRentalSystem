namespace CarRentalSystem.FrontOffice.Helpers
{
    /// <summary>
    /// Extension methods for easy access to authentication info from HttpContext
    /// </summary>
    public static class AuthHelper
    {
        /// <summary>
        /// Get the current authenticated user's ID from HttpContext items
        /// </summary>
        public static Guid? GetCurrentUserId(this HttpContext context)
        {
            if (context.Items.TryGetValue("UserId", out var userId) && userId is Guid id)
            {
                return id;
            }
            return null;
        }

        /// <summary>
        /// Check if the current request is from an authenticated user
        /// </summary>
        public static bool IsAuthenticated(this HttpContext context)
        {
            if (context.Items.TryGetValue("IsAuthenticated", out var isAuth) && isAuth is bool authenticated)
            {
                return authenticated;
            }
            return false;
        }

        /// <summary>
        /// Get the current user ID or throw if not authenticated
        /// </summary>
        public static Guid GetRequiredUserId(this HttpContext context)
        {
            var userId = context.GetCurrentUserId();
            if (userId == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }
            return userId.Value;
        }
    }
}
