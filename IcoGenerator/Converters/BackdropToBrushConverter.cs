using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using IcoGenerator.ViewModels;

namespace IcoGenerator.Converters;

public sealed class BackdropToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush Light = new(Color.FromRgb(0xF3, 0xF4, 0xF6));
    private static readonly SolidColorBrush Dark = new(Color.FromRgb(0x16, 0x17, 0x1B));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not PreviewBackdrop backdrop) return Application.Current.Resources["CheckerBrush"];

        return backdrop switch
        {
            PreviewBackdrop.Light => Light,
            PreviewBackdrop.Dark => Dark,
            _ => Application.Current.Resources["CheckerBrush"]
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
