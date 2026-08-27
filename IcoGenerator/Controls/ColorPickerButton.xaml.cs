using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace IcoGenerator.Controls;

public partial class ColorPickerButton : UserControl
{
    public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
        nameof(SelectedColor), typeof(Color), typeof(ColorPickerButton),
        new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    private static readonly Color[] Palette =
    {
        Colors.White, Colors.Black, Color.FromRgb(0xE5,0xE7,0xEB), Color.FromRgb(0x6B,0x72,0x80),
        Color.FromRgb(0xEF,0x44,0x44), Color.FromRgb(0xF9,0x73,0x16), Color.FromRgb(0xEA,0xB3,0x08),
        Color.FromRgb(0x22,0xC5,0x5E), Color.FromRgb(0x14,0xB8,0xA6), Color.FromRgb(0x06,0xB6,0xD4),
        Color.FromRgb(0x3B,0x82,0xF6), Color.FromRgb(0x63,0x66,0xF1), Color.FromRgb(0x8B,0x5C,0xF6),
        Color.FromRgb(0xD9,0x46,0xEF), Color.FromRgb(0xEC,0x48,0x99), Color.FromRgb(0x4F,0x8B,0xFF),
    };

    private bool _suppressUpdates;

    public ColorPickerButton()
    {
        InitializeComponent();
        BuildPalette();
        SyncControlsFromColor(SelectedColor);
    }

    private void BuildPalette()
    {
        foreach (var color in Palette)
        {
            var swatch = new Button
            {
                Width = 24,
                Height = 24,
                Margin = new Thickness(3),
                Padding = new Thickness(0),
                Background = new SolidColorBrush(color),
                BorderBrush = new SolidColorBrush(Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF)),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                Tag = color
            };
            swatch.SetResourceReference(Button.StyleProperty, "PaletteSwatchButtonStyle");
            swatch.Click += (_, _) =>
            {
                SelectedColor = color;
                PickerPopup.IsOpen = false;
            };
            PaletteItems.Items.Add(swatch);
        }
    }

    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorPickerButton picker)
            picker.SyncControlsFromColor((Color)e.NewValue);
    }

    private void SyncControlsFromColor(Color color)
    {
        _suppressUpdates = true;
        Swatch.Background = new SolidColorBrush(color);
        PreviewSwatch.Background = new SolidColorBrush(color);
        RSlider.Value = color.R;
        GSlider.Value = color.G;
        BSlider.Value = color.B;
        ASlider.Value = color.A;
        HexBox.Text = color.A < 255
            ? $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}"
            : $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        _suppressUpdates = false;
    }

    private void SwatchButton_Click(object sender, RoutedEventArgs e)
    {
        PickerPopup.IsOpen = true;
    }

    private void ChannelSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_suppressUpdates) return;
        SelectedColor = Color.FromArgb((byte)ASlider.Value, (byte)RSlider.Value, (byte)GSlider.Value, (byte)BSlider.Value);
    }

    private void HexBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ApplyHex();
            Keyboard.ClearFocus();
        }
    }

    private void HexBox_LostFocus(object sender, RoutedEventArgs e) => ApplyHex();

    private void ApplyHex()
    {
        if (_suppressUpdates) return;
        string text = HexBox.Text.Trim().TrimStart('#');

        try
        {
            byte a = 255, r, g, b;
            switch (text.Length)
            {
                case 6:
                    r = System.Convert.ToByte(text.Substring(0, 2), 16);
                    g = System.Convert.ToByte(text.Substring(2, 2), 16);
                    b = System.Convert.ToByte(text.Substring(4, 2), 16);
                    break;
                case 8:
                    a = System.Convert.ToByte(text.Substring(0, 2), 16);
                    r = System.Convert.ToByte(text.Substring(2, 2), 16);
                    g = System.Convert.ToByte(text.Substring(4, 2), 16);
                    b = System.Convert.ToByte(text.Substring(6, 2), 16);
                    break;
                default:
                    SyncControlsFromColor(SelectedColor);
                    return;
            }
            SelectedColor = Color.FromArgb(a, r, g, b);
        }
        catch (FormatException)
        {
            SyncControlsFromColor(SelectedColor);
        }
    }
}
