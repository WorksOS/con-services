using System;
using Draw = System.Drawing;
using VSS.TRex.Rendering.Abstractions;

namespace VSS.TRex.Rendering.Implementations.Core2
{
  public class Brush : IBrush, IDisposable
  {
    private readonly Draw.SolidBrush container;

    internal Draw.SolidBrush UnderlyingImplementation => container;

    internal Brush(Draw.Color color)
    {
      lock (RenderingLock.Lock)
      {
        container = new Draw.SolidBrush(color);
      }
    }

    internal Brush(Draw.Brush brush)
    {
      lock (RenderingLock.Lock)
      {
        container = (Draw.SolidBrush)brush;
      }
    }

    public Draw.Color Color
    {
      get
      {
        lock (RenderingLock.Lock)
        {
          return container.Color;
        }
      }

      set
      {
        lock (RenderingLock.Lock)
        {
          container.Color = value;
        }
      }
    }

    public void Dispose()
    {
      container?.Dispose();
    }
  }
}
