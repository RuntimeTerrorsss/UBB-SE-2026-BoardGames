// <copyright file="AccountProxyServiceAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BoardGames.Web.Infrastructure
{
    public sealed class AccountProxyServiceAdapter : IAccountProxyService
    {
        private readonly HttpClient httpClient;

        public AccountProxyServiceAdapter(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            if (this.httpClient.BaseAddress is null)
            {
                throw new InvalidOperationException("HttpClient BaseAddress must be configured.");
            }
        }

        public async Task<AccountProfileDTO> GetProfileAsync(Guid accountId)
        {
            using var response = await this.httpClient.GetAsync($"accounts/{accountId}");
            var profile = await HttpProxyClient.ReadAsync<AccountProfileDTO>(response);
            if (!string.IsNullOrEmpty(profile?.AvatarUrl))
            {
                profile.AvatarUrl = ApiUrlHelper.ToAbsoluteUrl(this.httpClient.BaseAddress!, profile.AvatarUrl);
            }

            return profile;
        }

        public async Task UpdateProfileAsync(Guid accountId, AccountProfileDTO updateData)
        {
            using var response = await this.httpClient.PutAsJsonAsync($"accounts/{accountId}", updateData);
            await HttpProxyClient.EnsureSuccessAsync(response);
        }

        public async Task UploadAvatarAsync(Guid accountId, string imagePath)
        {
            byte[] fileBytes = await File.ReadAllBytesAsync(imagePath);
            using var byteContent = new ByteArrayContent(fileBytes);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue(GuessContentType(imagePath));

            using var formData = new MultipartFormDataContent();
            formData.Add(byteContent, "file", Path.GetFileName(imagePath));

            using var response = await this.httpClient.PostAsync($"accounts/{accountId}/avatar", formData);
            await HttpProxyClient.EnsureSuccessAsync(response);
        }

        public async Task RemoveAvatarAsync(Guid accountId)
        {
            using var response = await this.httpClient.DeleteAsync($"accounts/{accountId}/avatar");
            await HttpProxyClient.EnsureSuccessAsync(response);
        }

        public async Task ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword)
        {
            var body = new ChangePasswordDTO
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmPassword = newPassword,
            };

            using var response = await this.httpClient.PutAsJsonAsync($"accounts/{accountId}/password", body);
            await HttpProxyClient.EnsureSuccessAsync(response);
        }

        private static string GuessContentType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream",
            };
        }
    }
}
