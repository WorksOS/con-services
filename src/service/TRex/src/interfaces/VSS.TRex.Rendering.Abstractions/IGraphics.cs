using Draw = System.Drawing;

namespace VSS.TRex.Rendering.Abstractions
{
  public interface IGraphics
  {
    void DrawRectangle(IPen pen, Draw.Rectangle rectangle);
    void DrawLine(IPen pen, int x1, int y1, int x2, int y2);
    void FillRectangle(IBrush pen, int x1, int y1, int x2, int y2);
    void DrawRectangle(IPen pen, int x1, int y1, int x2, int y2);
    void FillPolygon(IBrush brush, Draw.Point[] points);
    void DrawPolygon(IPen brush, Draw.Point[] points);
    void Clear(Draw.Color penColor);
  }
}
