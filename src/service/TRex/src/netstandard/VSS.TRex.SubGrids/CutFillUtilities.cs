using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;

namespace VSS.TRex.SubGrids
{
  /// <summary>
  /// Contains methods relevant to supporting Cut/Fill operations, such a computing cut/fill elevation subgrids
  /// </summary>
  public static class CutFillUtilities
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger("CutFillUtilities");

    /// <summary>
    /// Calculates a cut/fill subgrid from a production data elevation subgrid and an elevation subgrid computed from a referenced design,
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

      if (design == null)
      {
        return false;
      }

      design.GetDesignHeights(DataModelID, SubGrid.OriginAsCellAddress(), SubGrid.CellSize,
        out IClientHeightLeafSubGrid DesignElevations, out ProfilerRequestResult);

      if (ProfilerRequestResult != DesignProfilerRequestResult.OK && ProfilerRequestResult != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
      {
        Log.LogError($"Design profiler subgrid elevation request for {SubGrid.OriginAsCellAddress()} failed with error {ProfilerRequestResult}");
        return false;
      }

      ComputeCutFillSubgrid((IClientHeightLeafSubGrid) SubGrid, DesignElevations);

      return true;
    }

    /// <summary>
    /// Calculates a cut/fill subgrid from two elevation subgrids, replacing the elevations
    /// in the first subgrid with the resulting cut fill values
    /// </summary>
    /// <param name="subgrid1"></param>
    /// <param name="subgrid2"></param>
    public static void ComputeCutFillSubgrid(IClientHeightLeafSubGrid subgrid1, IClientHeightLeafSubGrid subgrid2)
    {
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
