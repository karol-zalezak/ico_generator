using System.IO;
using System.Windows.Media.Imaging;

namespace IcoGenerator.Services;

public static class PngExporter
{
    public static void Save(string path, BitmapSource bitmap)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        encoder.Save(fs);
    }
}
