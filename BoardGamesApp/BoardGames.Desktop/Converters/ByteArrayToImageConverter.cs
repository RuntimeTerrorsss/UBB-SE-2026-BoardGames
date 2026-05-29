using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;

namespace BoardGames.Desktop.Converters
{
    public sealed class ByteArrayToImageConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string url && !string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    return new BitmapImage(new Uri(url));
                }
                catch
                {
                    return null;
                }
            }

            if (value is not byte[] imageBytes || imageBytes.Length == 0)
            {
                return null;
            }

            try
            {
                var bitmapImage = new BitmapImage();
                using var stream = new InMemoryRandomAccessStream();
                stream.WriteAsync(imageBytes.AsBuffer()).AsTask().GetAwaiter().GetResult();
                stream.Seek(0);
                bitmapImage.SetSource(stream);
                return bitmapImage;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotSupportedException();
    }
}
