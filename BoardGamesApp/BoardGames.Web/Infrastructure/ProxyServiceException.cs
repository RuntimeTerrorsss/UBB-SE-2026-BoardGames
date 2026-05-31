using System;
using System.Net;

namespace BoardGames.Web.Infrastructure
{
    public sealed class ProxyServiceException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string? ApiErrorCode { get; }

        public ProxyServiceException(string message, HttpStatusCode statusCode, string? apiErrorCode)
            : base(message)
        {
            StatusCode = statusCode;
            ApiErrorCode = apiErrorCode;
        }

        public ProxyServiceException(string message, HttpStatusCode statusCode, string? apiErrorCode, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ApiErrorCode = apiErrorCode;
        }
    }
}
