using System;

namespace VSS.TRex.ElevationSmoothing
{
  public interface IConvolver<T>
  {
    int ContextSize { get; }

    void Convolve(int sizeX, int sizeY, Func<int, int, T> getValue, Action<int, int, T> setValue, T nullValue);
  }
}
