using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Audiosurf2_Tools.Converters;

public class ColorToBrushConverter2 : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Color.Parse(value?.ToString());
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return SolidColorBrush.Parse(value?.ToString());
    }
}