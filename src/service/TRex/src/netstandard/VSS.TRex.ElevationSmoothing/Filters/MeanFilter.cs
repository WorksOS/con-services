using System;

namespace VSS.TRex.ElevationSmoothing
{
  public class MeanFilter<T> : FilterConvolver<T>
  {
    protected static double[,] CreateFilter(int contextSize, double centerWeight)
    {
      if (contextSize < 3 || contextSize > 11)
      {
        throw new ArgumentException($"Context size of {contextSize} is out of range: 3..11");
      }

      var totalWeight = (contextSize * contextSize) - 1 + centerWeight;

      var result = new double[contextSize, contextSize];
      for (var i = 0; i < contextSize; i++)
      {
        for (var j = 0; j < contextSize; j++)
        {
          result[i, j] = 1.0d / totalWeight;
        }
      }

      result[contextSize / 2, contextSize / 2] = centerWeight / totalWeight;

      return result;
    }

    public MeanFilter(IConvolutionAccumulator<T> accumulator, int contextSize, bool updateNullValues, bool infillNullValuesOnly) : base(accumulator, CreateFilter(contextSize, 1), updateNullValues, infillNullValuesOnly)
    {
    }

    public MeanFilter(IConvolutionAccumulator<T> accumulator, double[,] filterMatrix, bool updateNullValues, bool infillNullValuesOnly) : base(accumulator, filterMatrix, updateNullValues, infillNullValuesOnly)
    {
    }
  }
}
