using System;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.ElevationSmoothing
{
  /// <summary>
  /// Implements the capability to take a sub grid tree containing queried elevation data and apply algorithmic smoothing to the data
  /// </summary>
  public class ElevationSmoother : IElevationSmoother
  {
    private readonly GenericSubGridTree_Float _sourceTree;
    private readonly IConvolutionTools<float> _convolutionTools;
    private readonly int _contextSize;

    private ElevationSmoother()
    {
    }

    public ElevationSmoother(GenericSubGridTree_Float sourceTree, IConvolutionTools<float> convolutionTools, int contextSize) : this()
    {
      if (_convolutionTools == null)
      {
        throw new ArgumentException("ConvolutionTools is null", nameof(convolutionTools));
      }

      _sourceTree = sourceTree;
      _convolutionTools = convolutionTools;
      _contextSize = contextSize;
    }

    public GenericSubGridTree_Float Smooth()
    {

      var result = new GenericSubGridTree_Float(_sourceTree.NumLevels, _sourceTree.CellSize);
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight);
      var convolver = new BasicSmoothingFilter<float>(accumulator, _contextSize);

      _sourceTree.ScanAllSubGrids(leaf =>
      {
        var smoothedLeaf = result.ConstructPathToCell(leaf.OriginX, leaf.OriginY, SubGridPathConstructionType.CreateLeaf) as GenericLeafSubGrid<float>;
        _convolutionTools.Convolve((GenericLeafSubGrid<float>)leaf, smoothedLeaf, convolver);
        return true;
      });

      return result;
    }
  }
}
