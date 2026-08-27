using System.Windows;
using System.Windows.Media;
using IcoGenerator.ViewModels;

namespace IcoGenerator.Models;

/// <summary>
/// Full description of an icon design. All spatial values (font size, offsets, corner
/// radius, stroke widths...) are expressed in pixels relative to the <see cref="ReferenceSize"/>
/// canvas and scaled proportionally when rendered at a different pixel size.
/// </summary>
public sealed class IconDesign : ObservableObject
{
    public const double ReferenceSize = 256.0;

    // ---- Text ---------------------------------------------------------

    private string _text = "A";
    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    private bool _uppercaseText;
    public bool UppercaseText
    {
        get => _uppercaseText;
        set => SetProperty(ref _uppercaseText, value);
    }

    private FontFamily _fontFamily = new("Segoe UI");
    public FontFamily FontFamily
    {
        get => _fontFamily;
        set => SetProperty(ref _fontFamily, value);
    }

    private double _fontSize = 140;
    public double FontSize
    {
        get => _fontSize;
        set => SetProperty(ref _fontSize, value);
    }

    private bool _bold = true;
    public bool Bold
    {
        get => _bold;
        set => SetProperty(ref _bold, value);
    }

    private bool _italic;
    public bool Italic
    {
        get => _italic;
        set => SetProperty(ref _italic, value);
    }

    private Color _textColor = Colors.White;
    public Color TextColor
    {
        get => _textColor;
        set => SetProperty(ref _textColor, value);
    }

    private bool _autoContrastText = true;
    public bool AutoContrastText
    {
        get => _autoContrastText;
        set => SetProperty(ref _autoContrastText, value);
    }

    private double _textOpacity = 1.0;
    public double TextOpacity
    {
        get => _textOpacity;
        set => SetProperty(ref _textOpacity, value);
    }

    private AnchorPosition _textAnchor = AnchorPosition.MiddleCenter;
    public AnchorPosition TextAnchor
    {
        get => _textAnchor;
        set => SetProperty(ref _textAnchor, value);
    }

    private double _offsetX;
    public double OffsetX
    {
        get => _offsetX;
        set => SetProperty(ref _offsetX, value);
    }

    private double _offsetY;
    public double OffsetY
    {
        get => _offsetY;
        set => SetProperty(ref _offsetY, value);
    }

    // ---- Background -----------------------------------------------------

    private BackgroundType _backgroundType = BackgroundType.LinearGradient;
    public BackgroundType BackgroundType
    {
        get => _backgroundType;
        set => SetProperty(ref _backgroundType, value);
    }

    private Color _backgroundColor1 = Color.FromRgb(0x4F, 0x8B, 0xFF);
    public Color BackgroundColor1
    {
        get => _backgroundColor1;
        set => SetProperty(ref _backgroundColor1, value);
    }

    private Color _backgroundColor2 = Color.FromRgb(0x8E, 0x5C, 0xF7);
    public Color BackgroundColor2
    {
        get => _backgroundColor2;
        set => SetProperty(ref _backgroundColor2, value);
    }

    private double _gradientAngle = 135;
    public double GradientAngle
    {
        get => _gradientAngle;
        set => SetProperty(ref _gradientAngle, value);
    }

    private string? _backgroundImagePath;
    public string? BackgroundImagePath
    {
        get => _backgroundImagePath;
        set => SetProperty(ref _backgroundImagePath, value);
    }

    private Stretch _backgroundImageStretch = Stretch.UniformToFill;
    public Stretch BackgroundImageStretch
    {
        get => _backgroundImageStretch;
        set => SetProperty(ref _backgroundImageStretch, value);
    }

    // ---- Shape ------------------------------------------------------------

    private double _cornerRadius = 56;
    public double CornerRadius
    {
        get => _cornerRadius;
        set => SetProperty(ref _cornerRadius, value);
    }

    private bool _showBorder;
    public bool ShowBorder
    {
        get => _showBorder;
        set => SetProperty(ref _showBorder, value);
    }

    private double _borderWidth = 6;
    public double BorderWidth
    {
        get => _borderWidth;
        set => SetProperty(ref _borderWidth, value);
    }

    private Color _borderColor = Colors.White;
    public Color BorderColor
    {
        get => _borderColor;
        set => SetProperty(ref _borderColor, value);
    }

    // ---- Shadow -------------------------------------------------------------

    private bool _showShadow;
    public bool ShowShadow
    {
        get => _showShadow;
        set => SetProperty(ref _showShadow, value);
    }

    private Color _shadowColor = Colors.Black;
    public Color ShadowColor
    {
        get => _shadowColor;
        set => SetProperty(ref _shadowColor, value);
    }

    private double _shadowBlur = 24;
    public double ShadowBlur
    {
        get => _shadowBlur;
        set => SetProperty(ref _shadowBlur, value);
    }

    private double _shadowDepth = 6;
    public double ShadowDepth
    {
        get => _shadowDepth;
        set => SetProperty(ref _shadowDepth, value);
    }

    private double _shadowDirection = 270;
    public double ShadowDirection
    {
        get => _shadowDirection;
        set => SetProperty(ref _shadowDirection, value);
    }

    private double _shadowOpacity = 0.55;
    public double ShadowOpacity
    {
        get => _shadowOpacity;
        set => SetProperty(ref _shadowOpacity, value);
    }

    public IconDesign Clone()
    {
        return (IconDesign)MemberwiseClone();
    }
}
