using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;

namespace VSS.TRex.SubGrids
{
  /// <summary>
  /// Contains methods relevant to supporting Cut/Fill operations, such a computing cut/fill elevation subgrids
  /// </summary>
  public static class CutFillUtilities
  {
    private static ILogger Log = Logging.Logger.CreateLogger("CutFillUtilities");

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
      IDesign design,
      Guid DataModelID,
      out DesignProfilerRequestResult ProfilerRequestResult)
    {
      ProfilerRequestResult = DesignProfilerRequestResult.UnknownError;

      if (design.GetDesignHeights(DataModelID, SubGrid.OriginAsCellAddress(), SubGrid.CellSize,
            out IClientHeightLeafSubGrid DesignElevations, out ProfilerRequestResult) == false)
      {
        if (ProfilerRequestResult != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
        {
          Log.LogError($"Design profiler subgrid elevation request for {SubGrid.OriginAsCellAddress()} failed with error {ProfilerRequestResult}");
          return false;
        }
      }

      ComputeCutFillSubgrid((IClientHeightLeafSubGrid) SubGrid, DesignElevations);

      return true;
    }

    /// <summary>
    /// Calculates a cutfill subgrid from two elevation subgrids, replacing the elevations
    /// in the first subgrid with the resulting cut fill values
    /// </summary>
    /// <param name="SubGrid1"></param>
    /// <param name="subgrid2"></param>
    public static void ComputeCutFillSubgrid(IClientHeightLeafSubGrid SubGrid1,
      IClientHeightLeafSubGrid subgrid2)
    {
      ClientHeightLeafSubGrid subgrid1 = SubGrid1 as ClientHeightLeafSubGrid;
      //ClientHeightLeafSubGrid subgrid2 = SubGrid2 as ClientHeightLeafSubGrid;

      SubGridUtilities.SubGridDimensionalIterator((I, J) =>
      {
        if (subgrid1.Cells[I, J] != Consts.NullHeight)
        {
          if (subgrid2.Cells[I, J] != Consts.NullHeight)
            subgrid1.Cells[I, J] = subgrid1.Cells[I, J] - subgrid2.Cells[I, J];
          else
            subgrid1.Cells[I, J] = Consts.NullHeight;
        }
      });
    }
  }
}
