using VSS.TRex.Filters;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Interfaces
{
  /// <summary>
  /// Interface for ClientLeafSubgrid derivations based on ClientLeafSubGrid
  /// </summary>
  public interface IClientLeafSubGrid : ILeafSubGrid
  {
    double CellSize { set; get; }

    GridDataType GridDataType { get; }

    PopulationControlFlags EventPopulationFlags { get; set; }

    bool WantsLiftProcessingResults();

    bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue);

    void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext Context);

    bool TopLayerOnly { get; set; }
  }
}