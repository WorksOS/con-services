using System;
using VSS.TRex.Rendering.Abstractions;
using Draw = System.Drawing;

namespace VSS.TRex.Rendering.Implementations.Framework
{
  public class Brush : IBrush, IDisposable
  {
    private readonly Draw.SolidBrush container;

    internal Draw.SolidBrush underlyingImplementation => container;

    internal Brush(Draw.Color color)
    {
      container = new Draw.SolidBrush(color);
    }

    internal Brush(Draw.Brush brush)
    {
      container = (Draw.SolidBrush)brush;
    }

    public Draw.Color Color
    {
      get => container.Color;
      set => container.Color = value;
    }


    public void Dispose()
    {
      container?.Dispose();
    }
  }
}
