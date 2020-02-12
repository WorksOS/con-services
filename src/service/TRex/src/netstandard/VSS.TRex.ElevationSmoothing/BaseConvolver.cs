using System;

namespace VSS.TRex.ElevationSmoothing
{
  /// <summary>
  /// Defines the basic convolution activity of iterating over a square context applying an averaging function over the
  /// values. ConvolveElement may be overridden to implenent other behaviours.
  /// </summary>
  public class BaseConvolver
  {
    protected readonly int _contextSize;

    protected Func<int, int, float> GetValue;
    protected Action<int, int, float> SetValue;
    protected float NullValue;

    public int ContextSize
    {
      get => _contextSize;
    }

    public BaseConvolver(int contextSize)
    {
      if (contextSize <= 1 || contextSize % 2 != 1)
      {
        throw new ArgumentException("Context size must be positive odd number greater than 1", nameof(_contextSize));
      }

      _contextSize = contextSize;
    }

    // Performs a convolution across a rectagular patch of values
    public virtual void ConvolveElement(int i, int j)
    {
      var contextOffset = _contextSize / 2;

      double sum = 0;
      var numValues = 0;
      for (int x = i - contextOffset, limitx = i + contextOffset; x <= limitx; x++)
      {
        for (int y = j - contextOffset, limity = j + contextOffset; y <= limity; y++)
        {
          var value = GetValue(x, y);

          if (value != NullValue)
          {
            sum += value;
            numValues++;
          }
        }
      }

      SetValue(i, j, numValues > 0 ? (float) (sum / numValues) : NullValue);
    }
  }
}
