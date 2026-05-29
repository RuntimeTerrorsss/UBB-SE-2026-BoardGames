// <copyright file="HttpProxyClient.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BoardGames.Web.Infrastructure
{
    internal static class HttpProxyClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await CreateExceptionAsync(response, cancellationToken);
            }

            T? value = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
            if (value is null)
            {
                throw new ProxyServiceException("The API returned an empty response body.", response.StatusCode, null);
            }

            return value;
        }

        public static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await CreateExceptionAsync(response, cancellationToken);
            }
        }

        private static async Task<ProxyServiceException> CreateExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            string message = $"API request failed ({(int)response.StatusCode} {response.ReasonPhrase}).";
            string? code = null;
            string body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    ApiErrorEnvelope? error = JsonSerializer.Deserialize<ApiErrorEnvelope>(body, JsonOptions);
                    if (!string.IsNullOrWhiteSpace(error?.Error))
                    {
                        message = error.Error!;
                    }

                    code = error?.Code;
                }
                catch (JsonException)
                {
                }
            }

            return new ProxyServiceException(message, response.StatusCode, code);
        }

        private sealed class ApiErrorEnvelope
        {
            public string? Error { get; set; }

            public string? Code { get; set; }
        }
    }
}
