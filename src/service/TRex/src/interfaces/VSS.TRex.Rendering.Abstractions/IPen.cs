using System;
using System.Drawing;

namespace VSS.TRex.Rendering.Abstractions
{
  public interface IPen : IDisposable
  {
    Color Color { get; set; }
    IBrush Brush { get; set; }
    float Width { get; set; }
  }
}
