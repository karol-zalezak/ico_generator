using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace IcoGenerator.Converters;

/// <summary>Visible when the bound enum equals ConverterParameter (comma-separated list of names allowed).</summary>
public sealed class EnumToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null) return Visibility.Collapsed;
        string current = value.ToString()!;
        var allowed = parameter.ToString()!.Split(',', StringSplitOptions.TrimEntries);
        return allowed.Contains(current) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
