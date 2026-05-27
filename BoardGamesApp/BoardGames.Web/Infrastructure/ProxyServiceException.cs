// <copyright file="ProxyServiceException.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

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
            this.StatusCode = statusCode;
            this.ApiErrorCode = apiErrorCode;
        }

        public ProxyServiceException(string message, HttpStatusCode statusCode, string? apiErrorCode, Exception innerException)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
            this.ApiErrorCode = apiErrorCode;
        }
    }
}
