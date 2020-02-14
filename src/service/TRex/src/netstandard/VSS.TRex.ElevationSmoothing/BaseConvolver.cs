using System;

namespace VSS.TRex.ElevationSmoothing
{
  /// <summary>
  /// Defines the basic convolution activity of iterating over a square context applying an averaging function over the
  /// values. ConvolveElement may be overridden to implenent other behaviours.
  /// </summary>
  public class BaseConvolver<T> : IBaseConvolver<T>
  {
    protected int _contextSize;
    protected readonly int _contextOffset;

    protected Func<int, int, T> GetValue;
    protected Action<int, int, T> SetValue;

    // Note: The explicit backing field is intentional to allow private code to reference it without the getter method
    protected readonly IConvolutionAccumulator<T> _accumulator;
    public IConvolutionAccumulator<T> Accumulator { get => _accumulator; }

    public int ContextSize
    {
      get => _contextSize;
      set
      {
        _contextSize = value;

        if (value <= 1 || value % 2 != 1)
        {
          throw new ArgumentException("Context size must be positive odd number greater than 1");
        }

        _contextSize = value;
      }
    }

    public BaseConvolver(IConvolutionAccumulator<T> accumulator)
    {
      _accumulator = accumulator;
    }

    public BaseConvolver(IConvolutionAccumulator<T> accumulator, int contextSize) : this(accumulator)
    {
      ContextSize = contextSize;
      _contextOffset = ContextSize / 2;
    }

    public virtual void ConvolveElement(int i, int j)
    {
      _accumulator.Clear();

      for (int x = i - _contextOffset, limitx = i + _contextOffset; x <= limitx; x++)
      {
        for (int y = j - _contextOffset, limity = j + _contextOffset; y <= limity; y++)
        {
          _accumulator.Accumulate(GetValue(x, y));
        }
      }

      SetValue(i, j, _accumulator.Result());
    }
  }
}
