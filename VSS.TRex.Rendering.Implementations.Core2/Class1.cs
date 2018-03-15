using System;
using CC = System.Drawing;
using VSS.TRex.Rendering.Abstractions;

namespace VSS.TRex.Rendering.Implementations.Core2
{
    public class RenderingFactory : IRenderingFactory
    {
      public IBitmap CreateBitmap(int x, int y)
      {
         return new Bitmap(x,y);
      }

      public IGraphics CreateGraphics(IBitmap bitmap)
      {
        throw new NotImplementedException();
      }

      public IPen CreatePen(System.Drawing.Color color)
      {
        throw new NotImplementedException();
      }

      public IBrush CreateBrush(System.Drawing.Color color)
      {
        throw new NotImplementedException();
      }

    }

  public class Graphics : IGraphics, IDisposable
  {

    private readonly CC.Graphics container;

    public void Dispose()
    {
      container?.Dispose();
    }

    public void DrawRectangle(IPen pen, CC.Rectangle rectangle)
    {
      throw new NotImplementedException();
    }

    public void DrawLine(IPen pen, int x1, int y1, int x2, int y2)
    {
      throw new NotImplementedException();
    }

    public void FillRectangle(IBrush pen, int x1, int y1, int x2, int y2)
    {
      throw new NotImplementedException();
    }

    public void DrawRectangle(IPen pen, int x1, int y1, int x2, int y2)
    {
      throw new NotImplementedException();
    }

    public void FillPolygon(IBrush brush, CC.Point[] points)
    {
      throw new NotImplementedException();
    }

    public void DrawPolygon(IPen brush, CC.Point[] points)
    {
      throw new NotImplementedException();
    }

    public void Clear(CC.Color penColor)
    {
      throw new NotImplementedException();
    }
  }

  public class Bitmap : IBitmap, IDisposable
  {

    private readonly CC.Bitmap container;

    internal Bitmap(int x, int y)
    {
      container = new CC.Bitmap(x, y);
    }

    public int Width => container.Width;
    public int Height => container.Height;

    public void Dispose()
    {
      container?.Dispose();
    }
  }

}
