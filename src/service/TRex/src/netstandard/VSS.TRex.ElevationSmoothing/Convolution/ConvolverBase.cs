using System;

namespace VSS.TRex.ElevationSmoothing
{
  /// <summary>
  /// Defines the basic convolution activity of iterating over a square context applying an averaging function over the
  /// values. ConvolveElement may be overridden to implenent other behaviours.
  /// </summary>
  public abstract class ConvolverBase<T> : IConvolver<T>
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

    protected ConvolverBase(IConvolutionAccumulator<T> accumulator)
    {
      _accumulator = accumulator;
    }

    protected ConvolverBase(IConvolutionAccumulator<T> accumulator, int contextSize) : this(accumulator)
    {
      ContextSize = contextSize;
      _contextOffset = ContextSize / 2;
    }

    public abstract void Convolve(int sizeX, int sizeY, Func<int, int, T> getValue, Action<int, int, T> setValue);

    public abstract void ConvolveElement(int i, int j);
  }
}
