using System;
using VSS.TRex.Rendering.Abstractions;

namespace VSS.TRex.Rendering.Implementations.Framework
{
  public class Pen : IPen, IDisposable
  {

    private readonly System.Drawing.Pen container;

    internal System.Drawing.Pen UnderlyingImplementation => container;

    internal Pen(System.Drawing.Color color)
    {
      container = new System.Drawing.Pen(color);
    }

    public System.Drawing.Color Color
    {
      get => container.Color;
      set => container.Color = value;
    }

    public IBrush Brush
    {
      get => new Brush(container.Brush);
      set => container.Brush = ((Brush)value).underlyingImplementation;
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