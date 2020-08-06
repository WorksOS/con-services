using System;
using Draw = System.Drawing;
using VSS.TRex.Rendering.Abstractions;
using System.Drawing.Imaging;

namespace VSS.TRex.Rendering.Implementations.Core2
{
  public class Bitmap : IBitmap, IDisposable
  {
    private readonly Draw.Bitmap container;

    internal Draw.Bitmap UnderlyingBitmap => container;

    public Bitmap(int x, int y)
    {
      lock (RenderingLock.Lock)
      {
        container = new Draw.Bitmap(x, y);
      }
    }

    //
    // Summary:
    //     Initializes a new instance of the System.Drawing.Bitmap class with the specified
    //     size, pixel format, and pixel data.
    //
    // Parameters:
    //   width:
    //     The width, in pixels, of the new System.Drawing.Bitmap.
    //
    //   height:
    //     The height, in pixels, of the new System.Drawing.Bitmap.
    //
    //   stride:
    //     Integer that specifies the byte offset between the beginning of one scan line
    //     and the next. This is usually (but not necessarily) the number of bytes in the
    //     pixel format (for example, 2 for 16 bits per pixel) multiplied by the width of
    //     the bitmap. The value passed to this parameter must be a multiple of four.
    //
    //   format:
    //     The pixel format for the new System.Drawing.Bitmap. This must specify a value
    //     that begins with Format.
    //
    //   scan0:
    //     Pointer to an array of bytes that contains the pixel data.
    //
    // Exceptions:
    //   T:System.ArgumentException:
    //     A System.Drawing.Imaging.PixelFormat value is specified whose name does not start
    //     with Format. For example, specifying System.Drawing.Imaging.PixelFormat.Gdi will
    //     cause an System.ArgumentException, but System.Drawing.Imaging.PixelFormat.Format48bppRgb
    //     will not.
    public Bitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0)
    {
      lock (RenderingLock.Lock)
      {
        container = new Draw.Bitmap(width, height, stride, format, scan0);
      }
    }

    public int Width => container.Width;
    public int Height => container.Height;

    public object GetBitmap() => UnderlyingBitmap;

    public void Dispose()
    {
      container?.Dispose();
    }
  }
}
