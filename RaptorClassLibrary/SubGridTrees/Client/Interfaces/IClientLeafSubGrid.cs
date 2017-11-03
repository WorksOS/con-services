using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.SubGridTrees.Interfaces
{
    /// <summary>
    /// Interface for ClientLeafSubgrid derivations based on ClientLeafSubGrid
    /// </summary>
    public interface IClientLeafSubGrid : ISubGrid, ILeafSubGrid
    {
        double CellSize { set; get; }

        GridDataType GridDataType { get; set; }

        bool WantsLiftProcessingResults();

        bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue);
        void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext Context);
    }
}
