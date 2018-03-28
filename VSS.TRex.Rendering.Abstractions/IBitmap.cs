using System;

namespace VSS.TRex.Rendering.Abstractions
{
  public interface IBitmap
  {
    int Width { get; }
    int Height { get; }
      Object GetBitmap();
  }
}