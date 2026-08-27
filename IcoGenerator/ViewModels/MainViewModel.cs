using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using IcoGenerator.Models;
using IcoGenerator.Services;
using Microsoft.Win32;

namespace IcoGenerator.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private static readonly int[] DefaultSizes = { 16, 24, 32, 48, 64, 128, 256 };

    public IconDesign Design { get; } = new();

    public ObservableCollection<IconSizeOption> Sizes { get; } = new();

    public IReadOnlyList<FontFamily> AvailableFonts { get; } =
        Fonts.SystemFontFamilies
            .OrderBy(f => f.Source, StringComparer.OrdinalIgnoreCase)
            .ToList();

    public IReadOnlyList<int> PreviewZoomLevels { get; } = new[] { 64, 128, 192, 256, 384, 512 };

    private PreviewBackdrop _previewBackdrop = PreviewBackdrop.Checkered;
    public PreviewBackdrop PreviewBackdrop
    {
        get => _previewBackdrop;
        set => SetProperty(ref _previewBackdrop, value);
    }

    private int _previewZoom = 256;
    public int PreviewZoom
    {
        get => _previewZoom;
        set
        {
            if (SetProperty(ref _previewZoom, value))
                RenderPreview();
        }
    }

    private BitmapSource? _previewImage;
    public BitmapSource? PreviewImage
    {
        get => _previewImage;
        private set => SetProperty(ref _previewImage, value);
    }

    private BitmapSource? _thumb16;
    public BitmapSource? Thumb16 { get => _thumb16; private set => SetProperty(ref _thumb16, value); }

    private BitmapSource? _thumb32;
    public BitmapSource? Thumb32 { get => _thumb32; private set => SetProperty(ref _thumb32, value); }

    private BitmapSource? _thumb48;
    public BitmapSource? Thumb48 { get => _thumb48; private set => SetProperty(ref _thumb48, value); }

    private BitmapSource? _thumb256;
    public BitmapSource? Thumb256 { get => _thumb256; private set => SetProperty(ref _thumb256, value); }

    private string _outputFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    public string OutputFolder
    {
        get => _outputFolder;
        set => SetProperty(ref _outputFolder, value);
    }

    private string _fileName = "icon";
    public string FileName
    {
        get => _fileName;
        set => SetProperty(ref _fileName, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public RelayCommand BrowseOutputFolderCommand { get; }
    public RelayCommand OpenOutputFolderCommand { get; }
    public RelayCommand BrowseBackgroundImageCommand { get; }
    public RelayCommand ClearBackgroundImageCommand { get; }
    public RelayCommand ExportIcoCommand { get; }
    public RelayCommand ExportPngCommand { get; }
    public RelayCommand CopyToClipboardCommand { get; }
    public RelayCommand RandomizeCommand { get; }
    public RelayCommand ResetCommand { get; }
    public RelayCommand SelectAllSizesCommand { get; }
    public RelayCommand SelectNoneSizesCommand { get; }
    public RelayCommand CornerRadiusPresetCommand { get; }

    private readonly DispatcherTimer _renderThrottle;
    private readonly Random _random = new();

    public MainViewModel()
    {
        foreach (int size in DefaultSizes)
        {
            var option = new IconSizeOption(size, size is 16 or 32 or 48 or 256);
            option.PropertyChanged += (_, _) => RaiseCanExecuteChangedForExport();
            Sizes.Add(option);
        }

        _renderThrottle = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(35)
        };
        _renderThrottle.Tick += (_, _) =>
        {
            _renderThrottle.Stop();
            RenderPreview();
        };

        Design.PropertyChanged += (_, _) => ScheduleRender();

        BrowseOutputFolderCommand = new RelayCommand(BrowseOutputFolder);
        OpenOutputFolderCommand = new RelayCommand(OpenOutputFolder, () => Directory.Exists(OutputFolder));
        BrowseBackgroundImageCommand = new RelayCommand(BrowseBackgroundImage);
        ClearBackgroundImageCommand = new RelayCommand(() => Design.BackgroundImagePath = null, () => Design.BackgroundImagePath != null);
        ExportIcoCommand = new RelayCommand(ExportIco, () => Sizes.Any(s => s.IsSelected));
        ExportPngCommand = new RelayCommand(ExportPng, () => Sizes.Any(s => s.IsSelected));
        CopyToClipboardCommand = new RelayCommand(CopyToClipboard);
        RandomizeCommand = new RelayCommand(Randomize);
        ResetCommand = new RelayCommand(ResetToDefaults);
        SelectAllSizesCommand = new RelayCommand(() => SetAllSizes(true));
        SelectNoneSizesCommand = new RelayCommand(() => SetAllSizes(false));
        CornerRadiusPresetCommand = new RelayCommand(p =>
        {
            if (p is string s && double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double value))
                Design.CornerRadius = value;
        });

        RenderPreview();
    }

    private void RaiseCanExecuteChangedForExport()
    {
        ExportIcoCommand.RaiseCanExecuteChanged();
        ExportPngCommand.RaiseCanExecuteChanged();
    }

    private void SetAllSizes(bool selected)
    {
        foreach (var s in Sizes) s.IsSelected = selected;
    }

    private void ScheduleRender()
    {
        _renderThrottle.Stop();
        _renderThrottle.Start();
    }

    private void RenderPreview()
    {
        PreviewImage = IconRenderer.Render(Design, PreviewZoom);
        Thumb16 = IconRenderer.Render(Design, 16);
        Thumb32 = IconRenderer.Render(Design, 32);
        Thumb48 = IconRenderer.Render(Design, 48);
        Thumb256 = IconRenderer.Render(Design, 256);
    }

    private void BrowseOutputFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select output folder",
            InitialDirectory = Directory.Exists(OutputFolder) ? OutputFolder : Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dialog.ShowDialog() == true)
        {
            OutputFolder = dialog.FolderName;
            OpenOutputFolderCommand.RaiseCanExecuteChanged();
        }
    }

    private void OpenOutputFolder()
    {
        if (Directory.Exists(OutputFolder))
        {
            Process.Start(new ProcessStartInfo(OutputFolder) { UseShellExecute = true });
        }
    }

    private void BrowseBackgroundImage()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose a background image",
            Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            Design.BackgroundImagePath = dialog.FileName;
            Design.BackgroundType = BackgroundType.Image;
            ClearBackgroundImageCommand.RaiseCanExecuteChanged();
        }
    }

    private IReadOnlyList<int> SelectedSizes() =>
        Sizes.Where(s => s.IsSelected).Select(s => s.Size).OrderBy(s => s).ToList();

    private void ExportIco()
    {
        var sizes = SelectedSizes();
        if (sizes.Count == 0) return;

        var dialog = new SaveFileDialog
        {
            Title = "Export .ico",
            Filter = "Icon file (*.ico)|*.ico",
            FileName = string.IsNullOrWhiteSpace(FileName) ? "icon" : FileName,
            InitialDirectory = Directory.Exists(OutputFolder) ? OutputFolder : Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var frames = sizes.Select(size => (size, (BitmapSource)IconRenderer.Render(Design, size))).ToList();
            IcoEncoder.Save(dialog.FileName, frames);
            OutputFolder = Path.GetDirectoryName(dialog.FileName) ?? OutputFolder;
            StatusMessage = $"Saved {Path.GetFileName(dialog.FileName)} ({sizes.Count} sizes)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
    }

    private void ExportPng()
    {
        var sizes = SelectedSizes();
        if (sizes.Count == 0) return;

        if (!Directory.Exists(OutputFolder))
        {
            try { Directory.CreateDirectory(OutputFolder); }
            catch (Exception ex)
            {
                StatusMessage = $"Could not create folder: {ex.Message}";
                return;
            }
        }

        try
        {
            string baseName = string.IsNullOrWhiteSpace(FileName) ? "icon" : FileName;
            foreach (int size in sizes)
            {
                var bmp = IconRenderer.Render(Design, size);
                string path = Path.Combine(OutputFolder, $"{baseName}_{size}x{size}.png");
                PngExporter.Save(path, bmp);
            }
            StatusMessage = $"Saved {sizes.Count} PNG file(s) to {OutputFolder}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
    }

    private void CopyToClipboard()
    {
        var bmp = IconRenderer.Render(Design, 256);
        Clipboard.SetImage(bmp);
        StatusMessage = "Copied 256×256 PNG to clipboard";
    }

    private static readonly (Color A, Color B)[] Palettes =
    {
        (Color.FromRgb(0x4F, 0x8B, 0xFF), Color.FromRgb(0x8E, 0x5C, 0xF7)),
        (Color.FromRgb(0x11, 0x99, 0x8A), Color.FromRgb(0x25, 0xD3, 0x66)),
        (Color.FromRgb(0xFF, 0x6B, 0x6B), Color.FromRgb(0xFF, 0xA5, 0x00)),
        (Color.FromRgb(0xF7, 0x4C, 0x9C), Color.FromRgb(0xB5, 0x30, 0xE0)),
        (Color.FromRgb(0x0F, 0x2C, 0x54), Color.FromRgb(0x4A, 0x69, 0x9C)),
        (Color.FromRgb(0xFF, 0xB7, 0x5E), Color.FromRgb(0xED, 0x4E, 0x6C)),
        (Color.FromRgb(0x00, 0xC9, 0xA7), Color.FromRgb(0x2F, 0x80, 0xED)),
        (Color.FromRgb(0x2E, 0x2E, 0x3A), Color.FromRgb(0x6E, 0x6E, 0x82)),
        (Color.FromRgb(0xF9, 0xD4, 0x23), Color.FromRgb(0xE9, 0x5B, 0x2B)),
        (Color.FromRgb(0x36, 0x1D, 0x64), Color.FromRgb(0x8A, 0x2B, 0xE2)),
    };

    private static readonly string[] SafeFonts =
    {
        "Segoe UI", "Segoe UI Semibold", "Arial", "Calibri", "Georgia",
        "Trebuchet MS", "Verdana", "Consolas", "Tahoma"
    };

    private void Randomize()
    {
        var (a, b) = Palettes[_random.Next(Palettes.Length)];
        Design.BackgroundColor1 = a;
        Design.BackgroundColor2 = b;
        Design.BackgroundType = _random.Next(3) switch
        {
            0 => BackgroundType.LinearGradient,
            1 => BackgroundType.RadialGradient,
            _ => BackgroundType.Solid
        };
        Design.GradientAngle = _random.Next(0, 8) * 45;
        Design.CornerRadius = new[] { 0, 24, 56, 96, 128 }[_random.Next(5)];

        string fontName = SafeFonts[_random.Next(SafeFonts.Length)];
        var found = AvailableFonts.FirstOrDefault(f => f.Source.Equals(fontName, StringComparison.OrdinalIgnoreCase));
        Design.FontFamily = found ?? new FontFamily(fontName);
        Design.Bold = _random.Next(2) == 0;
        Design.AutoContrastText = true;
        Design.TextAnchor = AnchorPosition.MiddleCenter;
        Design.OffsetX = 0;
        Design.OffsetY = 0;
    }

    private void ResetToDefaults()
    {
        var fresh = new IconDesign();
        Design.Text = fresh.Text;
        Design.UppercaseText = fresh.UppercaseText;
        Design.FontFamily = fresh.FontFamily;
        Design.FontSize = fresh.FontSize;
        Design.Bold = fresh.Bold;
        Design.Italic = fresh.Italic;
        Design.TextColor = fresh.TextColor;
        Design.AutoContrastText = fresh.AutoContrastText;
        Design.TextOpacity = fresh.TextOpacity;
        Design.TextAnchor = fresh.TextAnchor;
        Design.OffsetX = fresh.OffsetX;
        Design.OffsetY = fresh.OffsetY;
        Design.BackgroundType = fresh.BackgroundType;
        Design.BackgroundColor1 = fresh.BackgroundColor1;
        Design.BackgroundColor2 = fresh.BackgroundColor2;
        Design.GradientAngle = fresh.GradientAngle;
        Design.BackgroundImagePath = null;
        Design.BackgroundImageStretch = fresh.BackgroundImageStretch;
        Design.CornerRadius = fresh.CornerRadius;
        Design.ShowBorder = fresh.ShowBorder;
        Design.BorderWidth = fresh.BorderWidth;
        Design.BorderColor = fresh.BorderColor;
        Design.ShowShadow = fresh.ShowShadow;
        Design.ShadowColor = fresh.ShadowColor;
        Design.ShadowBlur = fresh.ShadowBlur;
        Design.ShadowDepth = fresh.ShadowDepth;
        Design.ShadowDirection = fresh.ShadowDirection;
        Design.ShadowOpacity = fresh.ShadowOpacity;
        StatusMessage = "Reset to defaults";
    }
}
