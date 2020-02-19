using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.DataSmoothing
{
  /// <summary>
  /// Implements the capability to take a sub grid tree containing queried elevation data and apply algorithmic smoothing to the data
  /// </summary>
  public class ElevationTreeSmoother : TreeDataSmoother<float>
  {
    public ElevationTreeSmoother(
      IConvolutionTools<float> convolutionTools, ConvolutionMaskSize contextSize, bool updateNullValues, bool infillNullValuesOnly)
      : base(convolutionTools, contextSize, 
        new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, contextSize),
        (accum, cSize) => new MeanFilter<float>(accum, cSize, updateNullValues, infillNullValuesOnly))
    {
    }
  }
}
