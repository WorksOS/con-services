using System;
using System.Drawing;

namespace VSS.TRex.Rendering.Abstractions
{
  public interface IBrush : IDisposable
  {
    Color Color { get; set; }
  }
}
