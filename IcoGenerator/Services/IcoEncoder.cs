using System.IO;
using System.Windows.Media.Imaging;

namespace IcoGenerator.Services;

/// <summary>
/// Writes a standard multi-resolution .ico file using PNG-compressed frames (the
/// "Vista style" ICO format), which every version of Windows since Vista understands
/// and which preserves full 32-bit alpha transparency at every size, including 256x256.
/// </summary>
public static class IcoEncoder
{
    public static void Save(string path, IReadOnlyList<(int Size, BitmapSource Bitmap)> frames)
    {
        if (frames.Count == 0)
            throw new ArgumentException("At least one frame is required to write an .ico file.", nameof(frames));

        var pngBlobs = new List<byte[]>(frames.Count);
        foreach (var (_, bitmap) in frames)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var ms = new MemoryStream();
            encoder.Save(ms);
            pngBlobs.Add(ms.ToArray());
        }

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(fs);

        // ICONDIR
        writer.Write((ushort)0);      // reserved
        writer.Write((ushort)1);      // type: 1 = icon
        writer.Write((ushort)frames.Count);

        int headerSize = 6;
        int entrySize = 16;
        int offset = headerSize + entrySize * frames.Count;

        // ICONDIRENTRY[]
        for (int i = 0; i < frames.Count; i++)
        {
            int size = frames[i].Size;
            byte dim = size >= 256 ? (byte)0 : (byte)size;

            writer.Write(dim);                    // width
            writer.Write(dim);                    // height
            writer.Write((byte)0);                 // color palette (0 = no palette)
            writer.Write((byte)0);                 // reserved
            writer.Write((ushort)1);               // color planes
            writer.Write((ushort)32);              // bits per pixel
            writer.Write((uint)pngBlobs[i].Length); // size of image data
            writer.Write((uint)offset);             // offset of image data

            offset += pngBlobs[i].Length;
        }

        // Image data
        foreach (var blob in pngBlobs)
        {
            writer.Write(blob);
        }
    }
}
