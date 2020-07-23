using System;
using Draw = System.Drawing;
using VSS.TRex.Rendering.Abstractions;
using System.Drawing.Imaging;

namespace VSS.TRex.Rendering.Implementations.Core2
{
  public class Graphics : IGraphics
  {
    private Draw.Graphics container;

    internal Graphics(Draw.Graphics graphics)
    {
      container = graphics;
    }

    public void Dispose()
    {
      container?.Dispose();
    }

    public void DrawRectangle(IPen pen, Draw.Rectangle rectangle)
    {
      container.DrawRectangle(((Pen)pen).UnderlyingImplementation,rectangle);
    }

    public void DrawLine(IPen pen, int x1, int y1, int x2, int y2)
    {
      container.DrawLine(((Pen)pen).UnderlyingImplementation,x1,y1,x2,y2);
    }

    public void FillRectangle(IBrush brush, int x1, int y1, int x2, int y2)
    {
      container.FillRectangle(((Brush)brush).UnderlyingImplementation, x1, y1, x2, y2);
    }

    public void DrawRectangle(IPen pen, int x1, int y1, int x2, int y2)
    {
      container.DrawRectangle(((Pen)pen).UnderlyingImplementation, x1, y1, x2, y2);
    }

    public void FillPolygon(IBrush brush, Draw.Point[] points)
    {
      container.FillPolygon(((Brush)brush).UnderlyingImplementation, points);
    }

    public void DrawPolygon(IPen pen, Draw.Point[] points)
    {
      container.DrawPolygon(((Pen)pen).UnderlyingImplementation, points);
    }

    public void Clear(Draw.Color penColor)
    {
      container.Clear(penColor);
    }

    /// <summary>
    /// Draws a 2D array of pixel colours on top of the current draw canvas where the 2D array is expressed as a simple vector
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="pixels">Contains an array of ARGB integers representing the pixels</param>
    public unsafe IBitmap DrawFromPixelArray(int width, int height, int[] pixels)
    {
      if (width * height != pixels.Length)
        throw new ArgumentException($"Dimensions of bitmap do not agree with size of pizel array: {width}x{height} vs {pixels.Length} pixels");

      var buffer = new byte[pixels.Length * 4];
      Buffer.BlockCopy(pixels, 0, buffer, 0, buffer.Length);

      fixed (byte* p = buffer)
      {
        var scan0 = (IntPtr)p;
        return new Bitmap(width, height, 4 * width, PixelFormat.Format32bppArgb, scan0);

        // (bmp.UnderlyingBitmap as System.Drawing.Bitmap).Save(@"c:\temp\TheBitmap.bmp");
      }
    }
  }
}
