using System;
using Draw = System.Drawing;
using VSS.TRex.Rendering.Abstractions;

namespace VSS.TRex.Rendering.Implementations.Core2
{
  public class Bitmap : IBitmap, IDisposable
  {
    private readonly Draw.Bitmap container;

    internal Draw.Bitmap UnderlyingBitmap => container;

    public Bitmap(int x, int y)
    {
      container = new Draw.Bitmap(x, y);
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
