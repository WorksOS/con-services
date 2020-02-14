using System;

namespace VSS.TRex.ElevationSmoothing
{
  /// <summary>
  /// Implements a classical base convoler that accepts a filter matrix and applies it to the data set
  /// to be convoled.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class Filter<T> : ConvolverBase<T>
  {
    public readonly double[,] FilterMatrix;

    public Filter(IConvolutionAccumulator<T> accumulator, double[,] filterMatrix) : base(accumulator)
    {
      FilterMatrix = filterMatrix;

      var majorDim = FilterMatrix.GetLength(0);
      var minorDim = FilterMatrix.GetLength(1);

      if (majorDim != minorDim)
      {
        throw new ArgumentException($"Major dimension ({majorDim}) and minor dimension ({minorDim}) of filterMatrix must be the same");
      }

      ContextSize = majorDim;
    }

    /// <summary>
    /// Performs a convolution across a rectagular patch of values
    /// </summary>
    /// <param name="sizeX"></param>
    /// <param name="sizeY"></param>
    /// <param name="getValue"></param>
    /// <param name="setValue"></param>
    public override void Convolve(int sizeX, int sizeY, Func<int, int, T> getValue, Action<int, int, T> setValue)
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

    /// <summary>
    /// Performs convolution on a single element in the data set
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    public override void ConvolveElement(int i, int j)
    {
      _accumulator.Clear();

      for (int x = i - _contextOffset, limitx = i + _contextOffset, majorIndex = 0; x <= limitx; x++, majorIndex++)
      {
        for (int y = j - _contextOffset, limity = j + _contextOffset, minorIndex = 0; y <= limity; y++, minorIndex++)
        {
          _accumulator.Accumulate(GetValue(x, y), FilterMatrix[majorIndex, minorIndex]);
        }
      }

      SetValue(i, j, _accumulator.Result());
    }
  }
}
