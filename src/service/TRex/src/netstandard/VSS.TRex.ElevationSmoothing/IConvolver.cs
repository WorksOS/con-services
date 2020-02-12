using System;

namespace VSS.TRex.ElevationSmoothing
{
  public interface IConvolver
  {
    int ContextSize { get; }

    void Convolve(int sizeX, int sizeY, Func<int, int, float> getValue, Action<int, int, float> setValue, float nullValue);
  }
}
