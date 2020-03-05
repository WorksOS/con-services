using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.DataSmoothing
{
  /// <summary>
  /// Implements the capability to take a 2D array containing queried elevation data and apply algorithmic smoothing to the data
  /// </summary>
  public class ElevationArraySmoother : ArrayDataSmoother<float>
  {
    public ElevationArraySmoother(
      IConvolutionTools<float> convolutionTools, ConvolutionMaskSize contextSize, NullInfillMode nullInfillMode)
      : base(convolutionTools, contextSize,
        new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, contextSize),
        (accum, cSize) => new MeanFilter<float>(accum, cSize, nullInfillMode))
    {
    }
  }
}
