using System;
using System.Collections.Generic;
using System.Text;
using VSS.Velociraptor.DesignProfiling;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Designs.Storage;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;

namespace VSS.VisionLink.Raptor.Utilities
{
    /// <summary>
    /// Contains methods relevant to supporting Cut/Fill operations, such a computing cut/fill elevation subgrids
    /// </summary>
    public static class CutFillUtilities
    {
        /// <summary>
        /// Calculates a cutfill subgrid from a production data elevation subgrid and an elevation subgrid computed from a referenced design,
        /// replacing the elevations in the first subgrid with the resulting cut fill values
        /// </summary>
        /// <param name="design"></param>
        /// <param name="SubGrid"></param>
        /// <param name="DataModelID"></param>
        /// <param name="ProfilerRequestResult"></param>
        /// <returns></returns>
        public static bool ComputeCutFillSubgrid(IClientLeafSubGrid SubGrid, 
                                                 Design design,                                                
                                                 long DataModelID,
                                                 out DesignProfilerRequestResult ProfilerRequestResult)
        {
            ProfilerRequestResult = DesignProfilerRequestResult.UnknownError;

            if (design.GetDesignHeights(DataModelID, SubGrid.OriginAsCellAddress(), SubGrid.CellSize, 
                                        out ClientHeightLeafSubGrid DesignElevations, out ProfilerRequestResult) == false)
            {
                if (ProfilerRequestResult != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
                {
                    // TODO readd when logging available
                    //SIGLogMessage.PublishNoODS(Self, Format('Design profiler subgrid elevation request for %s failed with error %d', [BaseScanSubGrid.OriginAsCellAddress.AsText, Ord(ProfilerRequestResult)]), slmcError);
                    return false;
                }
            }

            ComputeCutFillSubgrid(SubGrid, DesignElevations);

            return true;
        }

        /// <summary>
        /// Calculates a cutfill subgrid from two elevation subgrids, replacing the elevations
        /// in the first subgrid with the resulting cut fill values
        /// </summary>
        /// <param name="SubGrid1"></param>
        /// <param name="SubGrid2"></param>
        public static void ComputeCutFillSubgrid(IClientLeafSubGrid SubGrid1,
                                                 IClientLeafSubGrid SubGrid2)
        {
            ClientHeightLeafSubGrid subgrid1 = SubGrid1 as ClientHeightLeafSubGrid;
            ClientHeightLeafSubGrid subgrid2 = SubGrid2 as ClientHeightLeafSubGrid;

            SubGridUtilities.SubGridDimensionalIterator((I, J) =>
            {
                if (subgrid1.Cells[I, J] != Consts.NullHeight)
                {
                    if (subgrid2.Cells[I, J] != Consts.NullHeight)
                    {
                        subgrid1.Cells[I, J] = subgrid1.Cells[I, J] - subgrid2.Cells[I, J];
                    }
                    else
                    {
                        subgrid1.Cells[I, J] = Consts.NullHeight;
                    }
                }
            });
        }
    }
}
