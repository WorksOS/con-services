using System;

namespace VSS.TRex.ElevationSmoothing
{
  /// <summary>
  /// Implements a classical base convoler that accepts a filter matrix and applies it to the data set
  /// to be convoled.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class FilterConvolver<T> : ConvolverBase<T>
  {
    public readonly double[,] FilterMatrix;

    public FilterConvolver(IConvolutionAccumulator<T> accumulator, double[,] filterMatrix, bool updateNullValues) : base(accumulator)
    {
      _updateNullValues = updateNullValues;

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
    /// Performs convolution on a single element in the data set
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    public override void ConvolveElement(int i, int j)
    {
      _accumulator.Clear();
      _accumulator.ConvolutionSourceValue = GetValue(i, j);

      var convolutionSourceValueIsNull = _accumulator.ConvolutionSourceValueIsNull();

      if (!_updateNullValues && convolutionSourceValueIsNull)
      {
        return;
      }

      for (int x = i - _contextOffset, limitx = i + _contextOffset, majorIndex = 0; x <= limitx; x++, majorIndex++)
      {
        for (int y = j - _contextOffset, limity = j + _contextOffset, minorIndex = 0; y <= limity; y++, minorIndex++)
        {
          _accumulator.Accumulate(GetValue(x, y), FilterMatrix[majorIndex, minorIndex]);
        }
      }

      if (_updateNullValues && convolutionSourceValueIsNull)
      {
        SetValue(i, j, _accumulator.NullInfillResult(_contextSize));
      }
      else
      {
        SetValue(i, j, _accumulator.Result());
      }
    }
  }
}
