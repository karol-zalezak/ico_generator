using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace IcoGenerator.Converters;

/// <summary>Visible when the bound value is a non-null / non-empty string; set ConverterParameter="Invert" to flip.</summary>
public sealed class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool hasValue = value is string s ? !string.IsNullOrEmpty(s) : value is not null;
        bool invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);
        if (invert) hasValue = !hasValue;
        return hasValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
