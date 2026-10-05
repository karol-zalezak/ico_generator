using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using IcoGenerator.Models;

namespace IcoGenerator.Services;

/// <summary>
/// Builds the WPF visual tree for an <see cref="IconDesign"/> and rasterizes it at any
/// pixel size. Rendering directly at each target size (rather than downscaling one large
/// bitmap) keeps text and strokes crisp at small resolutions like 16x16.
/// </summary>
public static class IconRenderer
{
    private const double EdgePadding = 12;

    public static RenderTargetBitmap Render(IconDesign design, int pixelSize)
    {
        double scale = pixelSize / IconDesign.ReferenceSize;

        var root = new Grid
        {
            Width = pixelSize,
            Height = pixelSize,
            SnapsToDevicePixels = true
        };

        Brush backgroundBrush = BuildBackgroundBrush(design);

        var backgroundBorder = new Border
        {
            Width = pixelSize,
            Height = pixelSize,
            Background = backgroundBrush,
            CornerRadius = new CornerRadius(Math.Min(design.CornerRadius * scale, pixelSize / 2.0)),
        };

        if (design.ShowBorder && design.BorderWidth > 0)
        {
            backgroundBorder.BorderBrush = new SolidColorBrush(design.BorderColor);
            backgroundBorder.BorderThickness = new Thickness(Math.Max(1, design.BorderWidth * scale));
        }

        root.Children.Add(backgroundBorder);

        // Clip everything (including text) to the rounded-rect silhouette.
        root.Clip = new RectangleGeometry
        {
            Rect = new Rect(0, 0, pixelSize, pixelSize),
            RadiusX = Math.Min(design.CornerRadius * scale, pixelSize / 2.0),
            RadiusY = Math.Min(design.CornerRadius * scale, pixelSize / 2.0)
        };

        if (!string.IsNullOrEmpty(design.Text))
        {
            // No fixed Width/Height: a fixed size larger than the margin-reduced slot makes
            // WPF layout-clip this grid, which would cut text pushed past the edge.
            var safeArea = new Grid
            {
                Margin = new Thickness(EdgePadding * scale)
            };

            var textBlock = BuildTextBlock(design, scale, backgroundBrush);
            safeArea.Children.Add(textBlock);
            root.Children.Add(safeArea);
        }

        root.Measure(new Size(pixelSize, pixelSize));
        root.Arrange(new Rect(0, 0, pixelSize, pixelSize));
        root.UpdateLayout();

        var rtb = new RenderTargetBitmap(pixelSize, pixelSize, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(root);
        rtb.Freeze();
        return rtb;
    }

    private static TextBlock BuildTextBlock(IconDesign design, double scale, Brush backgroundBrush)
    {
        string text = design.UppercaseText ? design.Text.ToUpperInvariant() : design.Text;

        Color effectiveColor = design.AutoContrastText
            ? ComputeContrastingColor(backgroundBrush)
            : design.TextColor;

        var (hAlign, vAlign, textAlign) = AnchorToAlignment(design.TextAnchor);

        var textBlock = new TextBlock
        {
            Text = text,
            FontFamily = design.FontFamily,
            FontSize = Math.Max(1, design.FontSize * scale),
            FontWeight = design.Bold ? FontWeights.Bold : FontWeights.Normal,
            FontStyle = design.Italic ? FontStyles.Italic : FontStyles.Normal,
            Foreground = new SolidColorBrush(effectiveColor) { Opacity = design.TextOpacity },
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = textAlign,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = vAlign,
            TextTrimming = TextTrimming.None,
        };

        // Positive offsets push inward via Margin. Negative offsets push the text past
        // the edge via a TranslateTransform, because a negative Margin gets cut off by
        // WPF's layout clip at the safe-area bounds instead of the icon silhouette.
        double offsetX = design.OffsetX * scale;
        double offsetY = design.OffsetY * scale;
        double left = 0, top = 0, right = 0, bottom = 0, shiftX = 0, shiftY = 0;
        switch (hAlign)
        {
            case HorizontalAlignment.Left: left = Math.Max(0, offsetX); shiftX = Math.Min(0, offsetX); break;
            case HorizontalAlignment.Right: right = Math.Max(0, offsetX); shiftX = -Math.Min(0, offsetX); break;
        }
        switch (vAlign)
        {
            case VerticalAlignment.Top: top = Math.Max(0, offsetY); shiftY = Math.Min(0, offsetY); break;
            case VerticalAlignment.Bottom: bottom = Math.Max(0, offsetY); shiftY = -Math.Min(0, offsetY); break;
        }
        textBlock.Margin = new Thickness(left, top, right, bottom);
        if (shiftX != 0 || shiftY != 0)
            textBlock.RenderTransform = new TranslateTransform(shiftX, shiftY);

        if (design.ShowShadow)
        {
            textBlock.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = design.ShadowColor,
                BlurRadius = Math.Max(0, design.ShadowBlur * scale),
                ShadowDepth = Math.Max(0, design.ShadowDepth * scale),
                Direction = design.ShadowDirection,
                Opacity = design.ShadowOpacity,
                RenderingBias = System.Windows.Media.Effects.RenderingBias.Quality
            };
        }

        return textBlock;
    }

    private static (HorizontalAlignment h, VerticalAlignment v, TextAlignment t) AnchorToAlignment(AnchorPosition anchor)
    {
        return anchor switch
        {
            AnchorPosition.TopLeft => (HorizontalAlignment.Left, VerticalAlignment.Top, TextAlignment.Left),
            AnchorPosition.TopCenter => (HorizontalAlignment.Center, VerticalAlignment.Top, TextAlignment.Center),
            AnchorPosition.TopRight => (HorizontalAlignment.Right, VerticalAlignment.Top, TextAlignment.Right),
            AnchorPosition.MiddleLeft => (HorizontalAlignment.Left, VerticalAlignment.Center, TextAlignment.Left),
            AnchorPosition.MiddleCenter => (HorizontalAlignment.Center, VerticalAlignment.Center, TextAlignment.Center),
            AnchorPosition.MiddleRight => (HorizontalAlignment.Right, VerticalAlignment.Center, TextAlignment.Right),
            AnchorPosition.BottomLeft => (HorizontalAlignment.Left, VerticalAlignment.Bottom, TextAlignment.Left),
            AnchorPosition.BottomCenter => (HorizontalAlignment.Center, VerticalAlignment.Bottom, TextAlignment.Center),
            AnchorPosition.BottomRight => (HorizontalAlignment.Right, VerticalAlignment.Bottom, TextAlignment.Right),
            _ => (HorizontalAlignment.Center, VerticalAlignment.Center, TextAlignment.Center)
        };
    }

    public static Brush BuildBackgroundBrush(IconDesign design)
    {
        switch (design.BackgroundType)
        {
            case BackgroundType.Solid:
                return new SolidColorBrush(design.BackgroundColor1);

            case BackgroundType.LinearGradient:
            {
                var brush = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0.5),
                    EndPoint = new Point(1, 0.5),
                    RelativeTransform = new RotateTransform(design.GradientAngle, 0.5, 0.5)
                };
                brush.GradientStops.Add(new GradientStop(design.BackgroundColor1, 0.0));
                brush.GradientStops.Add(new GradientStop(design.BackgroundColor2, 1.0));
                return brush;
            }

            case BackgroundType.RadialGradient:
            {
                var brush = new RadialGradientBrush
                {
                    Center = new Point(0.5, 0.5),
                    GradientOrigin = new Point(0.5, 0.5),
                    RadiusX = 0.75,
                    RadiusY = 0.75
                };
                brush.GradientStops.Add(new GradientStop(design.BackgroundColor1, 0.0));
                brush.GradientStops.Add(new GradientStop(design.BackgroundColor2, 1.0));
                return brush;
            }

            case BackgroundType.Image when !string.IsNullOrEmpty(design.BackgroundImagePath) && File.Exists(design.BackgroundImagePath):
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(design.BackgroundImagePath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                return new ImageBrush(bitmap)
                {
                    Stretch = design.BackgroundImageStretch,
                    AlignmentX = AlignmentX.Center,
                    AlignmentY = AlignmentY.Center
                };
            }

            default:
                return new SolidColorBrush(design.BackgroundColor1);
        }
    }

    /// <summary>Samples a brush at low resolution and returns black or white, whichever contrasts best.</summary>
    private static Color ComputeContrastingColor(Brush brush)
    {
        const int sampleSize = 12;
        var rect = new Rectangle { Width = sampleSize, Height = sampleSize, Fill = brush };
        rect.Measure(new Size(sampleSize, sampleSize));
        rect.Arrange(new Rect(0, 0, sampleSize, sampleSize));

        var rtb = new RenderTargetBitmap(sampleSize, sampleSize, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(rect);

        var pixels = new byte[sampleSize * sampleSize * 4];
        rtb.CopyPixels(pixels, sampleSize * 4, 0);

        double totalLuminance = 0;
        int count = sampleSize * sampleSize;
        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte b = pixels[i];
            byte g = pixels[i + 1];
            byte r = pixels[i + 2];
            totalLuminance += 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        double avgLuminance = totalLuminance / count;
        return avgLuminance > 150 ? Colors.Black : Colors.White;
    }
}
