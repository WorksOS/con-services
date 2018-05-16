using System.Drawing;

namespace VSS.TRex.Rendering.Abstractions
{
  public interface IGraphics
  {
    void DrawRectangle(IPen pen, Rectangle rectangle);
    void DrawLine(IPen pen, int x1, int y1, int x2, int y2);
    void FillRectangle(IBrush pen, int x1, int y1, int x2, int y2);
    void DrawRectangle(IPen pen, int x1, int y1, int x2, int y2);
    void FillPolygon(IBrush brush, Point[] points);
    void DrawPolygon(IPen brush, Point[] points);
    void Clear(Color penColor);
  }
}