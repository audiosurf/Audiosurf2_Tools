using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Audiosurf2_Tools.Converters;

public class MoreFoldersPositionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i)
            if (i == -1)
                return "Last";
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
            if (s == "Last")
                return -1;
        return value;
    }
}