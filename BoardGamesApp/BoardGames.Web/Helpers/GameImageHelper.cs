// <copyright file="GameImageHelper.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Web.Helpers
{
    public static class GameImageHelper
    {
        public const string PlaceholderImageSource =
            "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 160 160'%3E%3Crect width='160' height='160' fill='%23e9ecef'/%3E%3Ccircle cx='58' cy='56' r='14' fill='%23ced4da'/%3E%3Cpath d='M38 118l30-34 20 23 16-18 18 18v11H38z' fill='%23adb5bd'/%3E%3C/svg%3E";

        public static string GetDisplaySource(GameSummaryDTO? game)
            => ResolveImageUrl(game?.ImageUrl);

        public static string GetDisplaySource(GameDTO? game)
        {
            if (game is null)
            {
                return PlaceholderImageSource;
            }

            if (game.Image is { Length: > 0 })
            {
                return ToDataUri(game.Image);
            }

            return ResolveImageUrl(game.ImageUrl);
        }

        public static string ResolveImageUrl(string? imageUrl)
        {
            string? resolved = MediaUrlHelper.ResolveMediaUrl(imageUrl);
            return string.IsNullOrWhiteSpace(resolved) ? PlaceholderImageSource : resolved;
        }

        private static string ToDataUri(byte[] imageBytes)
        {
            string mimeType = "image/jpeg";

            if (imageBytes.Length >= 4 &&
                imageBytes[0] == 0x89 &&
                imageBytes[1] == 0x50 &&
                imageBytes[2] == 0x4E &&
                imageBytes[3] == 0x47)
            {
                mimeType = "image/png";
            }
            else if (imageBytes.Length >= 3 &&
                     imageBytes[0] == 0x47 &&
                     imageBytes[1] == 0x49 &&
                     imageBytes[2] == 0x46)
            {
                mimeType = "image/gif";
            }
            else if (imageBytes.Length >= 2 &&
                     imageBytes[0] == 0x42 &&
                     imageBytes[1] == 0x4D)
            {
                mimeType = "image/bmp";
            }
            else if (imageBytes.Length >= 12 &&
                     imageBytes[0] == 0x52 &&
                     imageBytes[1] == 0x49 &&
                     imageBytes[2] == 0x46 &&
                     imageBytes[3] == 0x46 &&
                     imageBytes[8] == 0x57 &&
                     imageBytes[9] == 0x45 &&
                     imageBytes[10] == 0x42 &&
                     imageBytes[11] == 0x50)
            {
                mimeType = "image/webp";
            }

            return $"data:{mimeType};base64,{Convert.ToBase64String(imageBytes)}";
        }
    }
}
