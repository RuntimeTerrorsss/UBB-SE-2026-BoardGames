// <copyright file="MediaUrlHelper.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Web.Helpers
{
    public static class MediaUrlHelper
    {
        public static string? ResolveUserImageUrl(string? avatarUrl)
        {
            return ResolveMediaUrl(avatarUrl);
        }

        public static string? ResolveMessageImageUrl(string? imageUrl)
        {
            return ResolveMediaUrl(imageUrl);
        }

        private static string? ResolveMediaUrl(string? urlOrFileName)
        {
            if (string.IsNullOrWhiteSpace(urlOrFileName))
            {
                return null;
            }

            var trimmed = urlOrFileName.Trim();
            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absoluteUri)
                && (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
            {
                return trimmed;
            }

            var fileName = trimmed.TrimStart('/', '\\');
            return $"/images/{fileName}";
        }
    }
}
