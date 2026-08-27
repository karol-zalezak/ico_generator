using System.Globalization;
using System.Windows.Data;

namespace IcoGenerator.Converters;

/// <summary>Binds a ToggleButton/RadioButton IsChecked to one value of an enum-valued property, used to build segmented "radio" pickers (background type, text anchor...).</summary>
public sealed class EnumToBooleanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null) return false;
        return value.ToString() == parameter.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool isChecked || parameter is null) return Binding.DoNothing;
        return isChecked ? Enum.Parse(targetType, parameter.ToString()!) : Binding.DoNothing;
    }
}
