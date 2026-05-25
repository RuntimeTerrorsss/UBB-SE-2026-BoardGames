using System;
using System.IO;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BoardGames.Desktop.Converters
{
    public sealed class ByteArrayToImageConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not byte[] bytes || bytes.Length == 0)
            {
                return null;
            }

            var image = new BitmapImage();
            using var randomAccessStream = new InMemoryRandomAccessStream();
            using (var writer = new DataWriter(randomAccessStream.GetOutputStreamAt(0)))
            {
                writer.WriteBytes(bytes);
                writer.StoreAsync().AsTask().GetAwaiter().GetResult();
                writer.FlushAsync().AsTask().GetAwaiter().GetResult();
            }

            randomAccessStream.Seek(0);
            image.SetSource(randomAccessStream);
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotImplementedException();
    }
}
