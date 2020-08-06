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
      lock (RenderingLock.Lock)
      {
        container = new Draw.Pen(color);
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

    public IBrush Brush
    {
      get
      {
        lock (RenderingLock.Lock)
        {
          return new Brush(container.Brush);
        }
      }
      set
      {
        lock (RenderingLock.Lock)
        {
          container.Brush = ((Brush)value).UnderlyingImplementation;
        }
      }
    }

    public float Width
    {
      get
      {
        lock (RenderingLock.Lock)
        {
          return container.Width;
        }
      }
      set
      {
        lock (RenderingLock.Lock)
        {
          container.Width = value;
        }
      }
    }

    public void Dispose()
    {
      container?.Dispose();
    }
  }
}
