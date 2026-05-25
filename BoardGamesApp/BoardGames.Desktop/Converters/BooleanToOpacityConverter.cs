using System;
using Microsoft.UI.Xaml.Data;

namespace BoardGames.Desktop.Converters
{
    public sealed class BooleanToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            value is bool booleanValue && booleanValue ? 0.45d : 1.0d;

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotImplementedException();
    }
}
