using System;

namespace VSS.TRex.ElevationSmoothing
{
  /// <summary>
  /// Provides the ability to perform a convolution behaviour over a requested rectangular area of data with supplied
  /// getValue and setValue lambdas to access values in a convolver context holding source data and set values into the
  /// recipient result data.
  /// </summary>
  public class Convolver<T> : BaseConvolver<T>, IConvolver<T>
  {
    public Convolver(IConvolutionAccumulator<T> accumulator, int contextSize) : base(accumulator, contextSize)
    {
    }
     
    // Performs a convolution across a rectagular patch of values
    public void Convolve(int sizeX, int sizeY, Func<int, int, T> getValue, Action<int, int, T> setValue, T nullValue)
    {
      GetValue = getValue;
      SetValue = setValue;
      NullValue = nullValue;

      var contextOffset = _contextSize / 2;

      // Performs a convolution across a rectagular patch of values
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
