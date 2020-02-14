using System;

namespace VSS.TRex.ElevationSmoothing
{
  /// <summary>
  /// Implements a classical base convoler that accepts a filter matrix and applies it to the data set
  /// to be convoled.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class FilterConvolver<T> : BaseConvolver<T>
  {
    protected readonly double[,] FilterMatrix;

    public FilterConvolver(IConvolutionAccumulator<T> accumulator, double[,] filterMatrix) : base(accumulator)
    {
      FilterMatrix = filterMatrix;

      var majorDim = FilterMatrix.GetLength(0);
      var minorDim = FilterMatrix.GetLength(1);

      if (majorDim != minorDim)
      {
        throw new ArgumentException($"Major dimension ({majorDim}) and mino dimension ({minorDim}) or filterMatrix must be the same");
      }

      ContextSize = majorDim;
    }

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
