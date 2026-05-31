// <copyright file="ApiServiceBase.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.ProxyServices
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using BoardGames.Shared.DTO;

    public abstract class ApiServiceBase
    {
        private readonly IHttpClientFactory httpClientFactory;

        protected ApiServiceBase(IHttpClientFactory httpClientFactory)
            => this.httpClientFactory = httpClientFactory;

        protected HttpClient CreateClient() => this.httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);

        protected async Task<ServiceResult<T>> GetAsync<T>(string url)
        {
            try
            {
                var response = await CreateClient().GetAsync(url);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return ServiceResult<T>.Fail(ex.Message);
            }
        }

        protected async Task<ServiceResult<T>> PostAsync<T>(string url, object data)
        {
            try
            {
                var response = await CreateClient().PostAsJsonAsync(url, data);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return ServiceResult<T>.Fail(ex.Message);
            }
        }

        protected async Task<ServiceResult> PostAsync(string url, object data)
        {
            try
            {
                var response = await CreateClient().PostAsJsonAsync(url, data);
                if (!response.IsSuccessStatusCode)
                {
                    return ServiceResult.Fail(await ReadErrorMessageAsync(response), response.StatusCode);
                }

                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }

        protected async Task<ServiceResult<T>> PutAsync<T>(string url, object data)
        {
            try
            {
                var response = await CreateClient().PutAsJsonAsync(url, data);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return ServiceResult<T>.Fail(ex.Message);
            }
        }

        private async Task<ServiceResult<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return ServiceResult<T>.Fail(await ReadErrorMessageAsync(response), response.StatusCode);
            }

            var data = await response.Content.ReadFromJsonAsync<T>();
            return ServiceResult<T>.Ok(data!);
        }

        private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
        {
            try
            {
                var payload = await response.Content.ReadFromJsonAsync<ApiErrorPayload>();
                if (!string.IsNullOrWhiteSpace(payload?.Message))
                {
                    return payload.Message;
                }
            }
            catch
            {
                // Fall back to reason phrase below.
            }

            return response.ReasonPhrase ?? "API Error";
        }

        private sealed class ApiErrorPayload
        {
            public string? Message { get; set; }
        }
    }
}
