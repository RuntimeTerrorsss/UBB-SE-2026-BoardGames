using System;
using Microsoft.UI.Xaml.Data;

namespace BoardGames.Desktop.Converters
{
    public sealed class BooleanToOpacityConverter : IValueConverter
    {
        private const double FadedOpacity = 0.45;
        private const double FullOpacity = 1.0;

        public object Convert(object value, Type targetType, object parameter, string language) =>
            value is bool booleanValue && booleanValue ? FadedOpacity : FullOpacity;

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            value is double opacity && opacity < FullOpacity;
    }
}
