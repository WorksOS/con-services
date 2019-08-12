using System;

namespace VSS.TRex.Rendering.Abstractions
{
    public interface IBitmap : IDisposable
  {
    int Width { get; }
    int Height { get; }
      object GetBitmap();
  }
}
