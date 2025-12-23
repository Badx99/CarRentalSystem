using CarRentalSystem.FrontOffice.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CarRentalSystem.FrontOffice.Services
{
    public class AuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiClient _apiClient;
        private const string AuthCookieName = "AuthToken";
        private const string UserIdCookieName = "UserId";

        public AuthService(IHttpContextAccessor httpContextAccessor, ApiClient apiClient)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiClient = apiClient;
        }

        private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

        /// <summary>
        /// Attempt to login and store JWT token in HTTP-only cookie
        /// </summary>
        public async Task<bool> LoginAsync(string email, string password)
        {
            var response = await _apiClient.LoginAsync(email, password);
            if (response == null)
            {
                Console.WriteLine("[AuthService] Login failed: API returned null");
                return false;
            }

            if (string.IsNullOrEmpty(response.Token))
            {
                Console.WriteLine("[AuthService] Login failed: No token returned");
                return false;
            }

            StoreToken(response.Token, response.ExpiresAt, response.UserId);
            _apiClient.SetAuthToken(response.Token);
            return true;
        }

        /// <summary>
        /// Register a new customer and optionally log them in
        /// </summary>
        public async Task<bool> RegisterAsync(RegisterCustomerRequest request)
        {
            var response = await _apiClient.RegisterCustomerAsync(request);
            return response != null;
        }

        /// <summary>
        /// Clear authentication cookies and log out
        /// </summary>
        public Task LogoutAsync()
        {
            if (HttpContext != null)
            {
                HttpContext.Response.Cookies.Delete(AuthCookieName);
                HttpContext.Response.Cookies.Delete(UserIdCookieName);
            }
            _apiClient.ClearAuthToken();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Check if the current user is authenticated
        /// </summary>
        public Task<bool> IsAuthenticatedAsync()
        {
            var token = GetTokenFromCookie();
            if (string.IsNullOrEmpty(token))
                return Task.FromResult(false);

            // Check if token is expired
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                if (jwtToken.ValidTo < DateTime.UtcNow)
                    return Task.FromResult(false);

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Get the authentication token from cookie
        /// </summary>
        public Task<string?> GetTokenAsync()
        {
            return Task.FromResult(GetTokenFromCookie());
        }

        /// <summary>
        /// Get the current user's ID from the stored cookie or JWT token
        /// </summary>
        public Task<Guid?> GetCurrentUserIdAsync()
        {
            // First try the dedicated UserId cookie
            if (HttpContext?.Request.Cookies.TryGetValue(UserIdCookieName, out var userIdStr) == true)
            {
                if (Guid.TryParse(userIdStr, out var userId))
                    return Task.FromResult<Guid?>(userId);
            }

            // Fallback: parse from JWT token
            var token = GetTokenFromCookie();
            if (string.IsNullOrEmpty(token))
                return Task.FromResult<Guid?>(null);

            try
            {
                var userId = ExtractUserIdFromToken(token);
                return Task.FromResult(userId);
            }
            catch
            {
                return Task.FromResult<Guid?>(null);
            }
        }

        /// <summary>
        /// Initialize the ApiClient with stored token (call from middleware)
        /// </summary>
        public void InitializeFromCookie()
        {
            var token = GetTokenFromCookie();
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    
                    // Only set token if not expired
                    if (jwtToken.ValidTo > DateTime.UtcNow)
                    {
                        _apiClient.SetAuthToken(token);
                        
                        // Store user info in HttpContext.Items
                        var userId = ExtractUserIdFromToken(token);
                        if (userId.HasValue && HttpContext != null)
                        {
                            HttpContext.Items["UserId"] = userId.Value;
                            HttpContext.Items["IsAuthenticated"] = true;
                        }
                    }
                }
                catch
                {
                    // Invalid token, ignore
                }
            }
        }

        #region Private Methods

        private void StoreToken(string token, DateTime expiresAt, Guid userId)
        {
            if (HttpContext == null) return;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Use HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = expiresAt
            };

            HttpContext.Response.Cookies.Append(AuthCookieName, token, cookieOptions);
            HttpContext.Response.Cookies.Append(UserIdCookieName, userId.ToString(), cookieOptions);
        }

        private string? GetTokenFromCookie()
        {
            string? token = null;
            HttpContext?.Request.Cookies.TryGetValue(AuthCookieName, out token);
            return token;
        }

        private Guid? ExtractUserIdFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Try common claim types for user ID
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.NameIdentifier ||
                    c.Type == "sub" ||
                    c.Type == "nameid" ||
                    c.Type == "userId");

                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                    return userId;

                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
