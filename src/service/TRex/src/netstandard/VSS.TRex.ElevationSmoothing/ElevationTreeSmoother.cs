using VSS.TRex.SubGridTrees;
using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.ElevationSmoothing
{
  /// <summary>
  /// Implements the capability to take a sub grid tree containing queried elevation data and apply algorithmic smoothing to the data
  /// </summary>
  public class ElevationTreeSmoother : TreeDataSmoother<float>
  {
    public ElevationTreeSmoother(GenericSubGridTree<float, GenericLeafSubGrid<float>> source,
      IConvolutionTools<float> convolutionTools, int contextSize, bool updateNullValues, bool infillNullValuesOnly)
      : base(source, convolutionTools, contextSize, 
        new ConvolutionAccumulator_Float(CellPassConsts.NullHeight),
        (accum, cSize) => new MeanFilter<float>(accum, cSize, updateNullValues, infillNullValuesOnly))
    {
    }
  }
}
