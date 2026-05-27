using System;
using System.Collections.Generic;
using System.IO;
using BoardGames.Data.Constants;

namespace BoardGames.Api.Services
{
    internal static class GameInputHelper
    {
        private const int EmptyImageLength = 0;

        public static List<string> BuildValidationErrors(
            string gameName,
            decimal gamePrice,
            int minimumPlayerCount,
            int maximumPlayerCount,
            string gameDescription,
            int minimumNameLength,
            int maximumNameLength,
            decimal minimumAllowedPrice,
            int absoluteMinimumPlayerCount,
            int minimumDescriptionLength,
            int maximumDescriptionLength)
        {
            var gameValidationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(gameName) || gameName.Length < minimumNameLength || gameName.Length > maximumNameLength)
            {
                gameValidationErrors.Add(ValidationMessages2.NameLengthRange(minimumNameLength, maximumNameLength));
            }

            if (gamePrice < minimumAllowedPrice)
            {
                gameValidationErrors.Add(ValidationMessages2.PriceMinimum(minimumAllowedPrice));
            }

            if (minimumPlayerCount < absoluteMinimumPlayerCount)
            {
                gameValidationErrors.Add(ValidationMessages2.MinimumPlayerCount(absoluteMinimumPlayerCount));
            }

            if (maximumPlayerCount < minimumPlayerCount)
            {
                gameValidationErrors.Add(ValidationMessages2.MaximumPlayerCountComparedToMinimum);
            }

            if (string.IsNullOrWhiteSpace(gameDescription) || gameDescription.Length < minimumDescriptionLength || gameDescription.Length > maximumDescriptionLength)
            {
                gameValidationErrors.Add(ValidationMessages2.DescriptionLengthRange(minimumDescriptionLength, maximumDescriptionLength));
            }

            return gameValidationErrors;
        }

        public static byte[] EnsureImageOrDefault(byte[] gameImage, string applicationBaseDirectory)
        {
            if (gameImage != null && gameImage.Length > EmptyImageLength)
            {
                return gameImage;
            }

            try
            {
                string defaultGameImagePath = Path.Combine(applicationBaseDirectory, "Assets", "default-game-placeholder.jpg");
                return File.ReadAllBytes(defaultGameImagePath);
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }
    }
}
