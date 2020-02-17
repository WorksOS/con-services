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
    protected int _contextOffset;

    protected bool _updateNullValues;
    public bool UpdateNullValues => _updateNullValues;

    protected bool _infillNullValuesOnly;
    public bool InfillNullValuesOnly => _infillNullValuesOnly;

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

        _contextOffset = _contextSize / 2;
      }
    }

    protected ConvolverBase(IConvolutionAccumulator<T> accumulator)
    {
      _accumulator = accumulator;
    }

    protected ConvolverBase(IConvolutionAccumulator<T> accumulator, int contextSize, bool updateNullValues, bool infillNullValuesOnly) : this(accumulator)
    {
      ContextSize = contextSize;
      _updateNullValues = updateNullValues;
      _infillNullValuesOnly = infillNullValuesOnly;
    }


    public abstract void ConvolveElement(int i, int j);

    /// <summary>
    /// Performs a convolution across a rectagular patch of values
    /// </summary>
    /// <param name="sizeX"></param>
    /// <param name="sizeY"></param>
    /// <param name="getValue"></param>
    /// <param name="setValue"></param>
    public void Convolve(int sizeX, int sizeY, Func<int, int, T> getValue, Action<int, int, T> setValue)
    {
      GetValue = getValue;
      SetValue = setValue;

      for (var i = 0; i < sizeX; i++)
      {
        for (var j = 0; j < sizeY; j++)
        {
          ConvolveElement(i, j);
        }
      }
    }
  }
}
