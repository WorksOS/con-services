using System;
using Draw = System.Drawing;
using VSS.TRex.Rendering.Abstractions;

namespace VSS.TRex.Rendering.Implementations.Core2
{
  public class Pen : IPen, IDisposable
  {
    private readonly Draw.Pen container;

    internal Draw.Pen UnderlyingImplementation => container;

    internal Pen(Draw.Color color)
    {
      container = new Draw.Pen(color);
    }

    public Draw.Color Color
    {
      get => container.Color;
      set => container.Color = value;
    }

    public IBrush Brush
    {
      get => new Brush(container.Brush);
      set => container.Brush = ((Brush)value).UnderlyingImplementation;
    }

    public float Width
    {
      get => container.Width;
      set => container.Width = value;
    }

    public void Dispose()
    {
      container?.Dispose();
    }
  }
}
