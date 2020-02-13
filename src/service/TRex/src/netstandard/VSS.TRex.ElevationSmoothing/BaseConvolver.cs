using System;

namespace VSS.TRex.ElevationSmoothing
{
  /// <summary>
  /// Defines the basic convolution activity of iterating over a square context applying an averaging function over the
  /// values. ConvolveElement may be overridden to implenent other behaviours.
  /// </summary>
  public class BaseConvolver<T> : IBaseConvolver<T>
  {
    protected readonly int _contextSize;

    protected Func<int, int, T> GetValue;
    protected Action<int, int, T> SetValue;

    // Note: The explicit backing field is intentional to allow private code to reference it without the getter method
    private readonly IConvolutionAccumulator<T> _accumulator;
    public IConvolutionAccumulator<T> Accumulator { get => _accumulator; }

    public int ContextSize
    {
      get => _contextSize;
    }

    public BaseConvolver(IConvolutionAccumulator<T> accumulator, int contextSize)
    {
      if (contextSize <= 1 || contextSize % 2 != 1)
      {
        throw new ArgumentException("Context size must be positive odd number greater than 1", nameof(_contextSize));
      }

      _contextSize = contextSize;
      _accumulator = accumulator;
    }

    public virtual void ConvolveElement(int i, int j)
    {
      var contextOffset = _contextSize / 2;

      _accumulator.Clear();

      for (int x = i - contextOffset, limitx = i + contextOffset; x <= limitx; x++)
      {
        for (int y = j - contextOffset, limity = j + contextOffset; y <= limity; y++)
        {
          _accumulator.Accumulate(GetValue(x, y));
        }
      }

      SetValue(i, j, _accumulator.Result());
    }
  }
}
