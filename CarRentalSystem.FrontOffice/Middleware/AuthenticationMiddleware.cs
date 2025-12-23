using CarRentalSystem.FrontOffice.Services;

namespace CarRentalSystem.FrontOffice.Middleware
{
    /// <summary>
    /// Middleware to check for authentication cookies and initialize the ApiClient
    /// Register in Program.cs: app.UseMiddleware<AuthenticationMiddleware>();
    /// (before UseAuthorization)
    /// </summary>
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AuthService authService)
        {
            // Initialize auth from cookie on each request
            authService.InitializeFromCookie();

            await _next(context);
        }
    }

    /// <summary>
    /// Extension method for easy middleware registration
    /// </summary>
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}
