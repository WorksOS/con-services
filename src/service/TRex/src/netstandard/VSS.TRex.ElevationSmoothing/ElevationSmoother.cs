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
    private readonly IElevationSmootherAlgorithm _algorithm;
    private readonly int _contextSize;

    private ElevationSmoother()
    {
    }

    public ElevationSmoother(GenericSubGridTree_Float sourceTree, IElevationSmootherAlgorithm algorithm, int contextSize) : this()
    {
      _sourceTree = sourceTree;
      _algorithm = algorithm;
      _contextSize = contextSize;
    }

    public GenericSubGridTree_Float Smooth()
    {
      if (_algorithm == null)
        throw new ArgumentException("Smoother algorithm is null");

      var result = new GenericSubGridTree_Float(_sourceTree.NumLevels, _sourceTree.CellSize);
      var convolver = new Convolver(_contextSize);

      _sourceTree.ScanAllSubGrids(leaf =>
      {
        var smoothedLeaf = result.ConstructPathToCell(leaf.OriginX, leaf.OriginY, SubGridPathConstructionType.CreateLeaf) as GenericLeafSubGrid_Float;
        _algorithm.SmoothLeaf((GenericLeafSubGrid_Float)leaf, smoothedLeaf, convolver);
        return true;
      });

      return result;
    }
  }
}
