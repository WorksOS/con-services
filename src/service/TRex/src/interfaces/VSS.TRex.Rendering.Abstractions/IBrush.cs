using Draw = System.Drawing;

namespace VSS.TRex.Rendering.Abstractions
{
  public interface IBrush
  {
    Draw.Color Color { get; set; }
  }
}
