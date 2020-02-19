using System;

namespace VSS.TRex.DataSmoothing
{
  public class MeanFilter<T> : FilterConvolver<T>
  {
    protected static double[,] CreateFilter(ConvolutionMaskSize contextSize, double centerWeight)
    {
      var contextSizeAsInt = (int) contextSize;

      if (contextSizeAsInt < 3 || contextSizeAsInt > 11)
      {
        throw new ArgumentException($"Context size of {contextSize} is out of range: 3..11");
      }

      var totalWeight = (contextSizeAsInt * contextSizeAsInt) - 1 + centerWeight;

      var result = new double[contextSizeAsInt, contextSizeAsInt];
      for (var i = 0; i < contextSizeAsInt; i++)
      {
        for (var j = 0; j < contextSizeAsInt; j++)
        {
          result[i, j] = 1.0d / totalWeight;
        }
      }

      result[contextSizeAsInt / 2, contextSizeAsInt / 2] = centerWeight / totalWeight;

      return result;
    }

    public MeanFilter(IConvolutionAccumulator<T> accumulator, ConvolutionMaskSize contextSize, NullInfillMode nullInfillMode) : base(accumulator, CreateFilter(contextSize, 1), nullInfillMode)
    {
    }

    public MeanFilter(IConvolutionAccumulator<T> accumulator, double[,] filterMatrix, NullInfillMode nullInfillMode) : base(accumulator, filterMatrix, nullInfillMode)
    {
    }
  }
}
