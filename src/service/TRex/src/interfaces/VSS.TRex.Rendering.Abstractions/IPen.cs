using Draw = System.Drawing;

namespace VSS.TRex.Rendering.Abstractions
{
  public interface IPen
  {
    Draw.Color Color { get; set; }
    IBrush Brush { get; set; }
    float Width { get; set; }
  }
}
