using VSS.TRex.Caching;
using VSS.TRex.Events.Models;
using VSS.TRex.Filters.Models;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Client.Interfaces
{
  /// <summary>
  /// Interface for ClientLeafSubgrid derivations based on ClientLeafSubGrid
  /// </summary>
  public interface IClientLeafSubGrid : ILeafSubGrid, ITRexMemoryCacheItem
  {
    double CellSize { set; get; }

    uint IndexOriginOffset { get; set; }

    GridDataType GridDataType { get; }

    PopulationControlFlags EventPopulationFlags { get; set; }

    bool WantsLiftProcessingResults();

    void Assign(IClientLeafSubGrid source);

    bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue);

    void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext Context);

    bool TopLayerOnly { get; set; }

    /// <summary>
    /// Fills the contents of the client leaf subgrid with a known, non-null test pattern of values
    /// </summary>
    void FillWithTestPattern();

    /// <summary>
    /// Determines if the leaf content of this subgrid is equal to 'other'
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    bool LeafContentEquals(IClientLeafSubGrid other);

    void AssignFromCachedPreProcessedClientSubgrid(ISubGrid source, SubGridTreeBitmapSubGridBits map);

    SubGridTreeBitmapSubGridBits ProdDataMap { get; set; }

    /// <summary>
    /// Existence map of cells matching current filter settings
    /// </summary>
    SubGridTreeBitmapSubGridBits FilterMap { get; set; }
  }
}
