using System;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.ElevationSmoothing
{
  /// <summary>
  /// Implements the capability to take a sub grid tree containing queried elevation data and apply algorithmic smoothing to the data
  /// </summary>
  public class ElevationSmoother : IElevationSmoother
  {
    private readonly GenericSubGridTree_Float _sourceTree;

    private ElevationSmoother()
    {
    }

    public ElevationSmoother(GenericSubGridTree_Float sourceTree) : this()
    {
      _sourceTree = sourceTree;
    }

    public GenericSubGridTree_Float Smooth(IElevationSmootherAlgorithm algorithm)
    {
      if (algorithm == null)
        throw new ArgumentException("Smoother algorithm is null");

      var result = new GenericSubGridTree_Float(_sourceTree.NumLevels, _sourceTree.CellSize);

      _sourceTree.ScanAllSubGrids(leaf =>
      {
        var smoothedLeaf = result.ConstructPathToCell(leaf.OriginX, leaf.OriginY, SubGridPathConstructionType.CreateLeaf);
        algorithm.SmoothLeaf((GenericLeafSubGrid_Float)leaf, (GenericLeafSubGrid_Float)smoothedLeaf);
        return true;
      });

      return result;
    }
  }
}
