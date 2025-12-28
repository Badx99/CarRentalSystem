using System;
using System.Net;

namespace CarRentalSystem.BackOffice.Services
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ErrorContent { get; }

        public ApiException(string message, HttpStatusCode statusCode, string errorContent) 
            : base(message)
        {
            StatusCode = statusCode;
            ErrorContent = errorContent;
        }

        public string? GetErrorMessage()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ErrorContent)) return null;
                
                var errorObj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ErrorContent);
                return errorObj?.error?.ToString() ?? errorObj?.message?.ToString() ?? ErrorContent;
            }
            catch
            {
                return ErrorContent;
            }
        }
    }
}
