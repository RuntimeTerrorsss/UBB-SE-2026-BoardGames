// <copyright file="GameImage.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.Helpers
{
    /// <summary>
    /// Provides helpers to transform raw image bytes into UI bitmap sources.
    /// </summary>
    internal static class GameImage
    {
        private const long StartOfStreamPosition = 0;

        /// <summary>
        /// Converts a byte array into a <see cref="BitmapImage"/> that can be used in WinUI bindings.
        /// </summary>
        /// <param name="imageBytes">Raw image bytes.</param>
        /// <returns>A bitmap image instance, or <see langword="null"/> when bytes are empty.</returns>
        public static async Task<BitmapImage?> ToBitmapImageAsync(byte[]? imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
            {
                return null;
            }

            using var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(imageBytes.AsBuffer());
            stream.Seek(StartOfStreamPosition);

            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);
            return bitmap;
        }
    }
}
