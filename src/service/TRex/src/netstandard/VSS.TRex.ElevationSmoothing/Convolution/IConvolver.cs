using System;

namespace VSS.TRex.DataSmoothing
{
  public interface IConvolver<T>
  {
    int ContextSize { get; }

    /// <summary>
    /// Determines if null values in the source data should be overwritten via convolution of the surrounding values
    /// </summary>
    bool UpdateNullValues { get; }

    IConvolutionAccumulator<T> Accumulator { get; }

    void Convolve(int sizeX, int sizeY, Func<int, int, T> getValue, Action<int, int, T> setValue);

    void ConvolveElement(int i, int j);
  }
}
