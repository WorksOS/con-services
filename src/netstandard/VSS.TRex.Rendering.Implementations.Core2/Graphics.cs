using System;
using Draw = System.Drawing;
using VSS.TRex.Rendering.Abstractions;

namespace VSS.TRex.Rendering.Implementations.Core2
{
  public class Graphics : IGraphics, IDisposable
  {
    private readonly Draw.Graphics container;

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
  }
}
