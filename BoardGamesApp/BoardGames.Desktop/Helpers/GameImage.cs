
namespace BoardGames.Desktop.Helpers
{
    internal static class GameImage
    {
        private const long StartOfStreamPosition = 0;
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
