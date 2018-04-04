using System;
using VSS.TRex.Rendering.Abstractions;

namespace VSS.TRex.Rendering.Implementations.Core2
{
  public class Graphics : IGraphics, IDisposable
  {

    private readonly System.Drawing.Graphics container;

    internal Graphics(System.Drawing.Graphics graphics)
    {
      container = graphics;
    }

    public void Dispose()
    {
      container?.Dispose();
    }

    public void DrawRectangle(IPen pen, System.Drawing.Rectangle rectangle)
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

    public void FillPolygon(IBrush brush, System.Drawing.Point[] points)
    {
      container.FillPolygon(((Brush)brush).UnderlyingImplementation, points);
    }

    public void DrawPolygon(IPen pen, System.Drawing.Point[] points)
    {
      container.DrawPolygon(((Pen)pen).UnderlyingImplementation, points);
    }

    public void Clear(System.Drawing.Color penColor)
    {
      container.Clear(penColor);
    }
  }
}