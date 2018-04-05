using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.SubGridTrees.Interfaces
{
    /// <summary>
    /// Interface for ClientLeafSubgrid derivations based on ClientLeafSubGrid
    /// </summary>
    public interface IClientLeafSubGrid : ILeafSubGrid
    {
        double CellSize { set; get; }

        GridDataType GridDataType { get; }

        bool WantsLiftProcessingResults();

        bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue);
        void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext Context);
    }
}
