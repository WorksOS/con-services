using System;
using VSS.TRex.Rendering.Abstractions;

namespace VSS.TRex.Rendering.Implementations.Framework
{
  public class Brush : IBrush, IDisposable
  {
    private readonly System.Drawing.SolidBrush container;

    internal System.Drawing.SolidBrush underlyingImplementation => container;

    internal Brush(System.Drawing.Color color)
    {
      container = new System.Drawing.SolidBrush(color);
    }

    internal Brush(System.Drawing.Brush brush)
    {
      container = (System.Drawing.SolidBrush)brush;
    }

    public System.Drawing.Color Color
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