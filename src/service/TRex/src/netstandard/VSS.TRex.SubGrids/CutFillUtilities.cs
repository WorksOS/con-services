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
  /// Contains methods relevant to supporting Cut/Fill operations, such a computing cut/fill elevation sub grids
  /// </summary>
  public static class CutFillUtilities
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger("CutFillUtilities");

    /// <summary>
    /// Calculates a cut/fill sub grid from a production data elevation sub grid and an elevation sub grid computed from a referenced design,
    /// replacing the elevations in the first sub grid with the resulting cut fill values
    /// </summary>
    /// <param name="design"></param>
    /// <param name="SubGrid"></param>
    /// <param name="DataModelID"></param>
    /// <param name="ProfilerRequestResult"></param>
    /// <returns></returns>
    public static bool ComputeCutFillSubGrid(IClientLeafSubGrid SubGrid,
      IDesign design,
      Guid DataModelID,
      out DesignProfilerRequestResult ProfilerRequestResult)
    {
      ProfilerRequestResult = DesignProfilerRequestResult.UnknownError;

      if (design == null)
        return false;

      design.GetDesignHeights(DataModelID, SubGrid.OriginAsCellAddress(), SubGrid.CellSize,
        out IClientHeightLeafSubGrid DesignElevations, out ProfilerRequestResult);

      if (ProfilerRequestResult != DesignProfilerRequestResult.OK && ProfilerRequestResult != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
      {
        Log.LogError($"Design profiler sub grid elevation request for {SubGrid.OriginAsCellAddress()} failed with error {ProfilerRequestResult}");
        return false;
      }

      ComputeCutFillSubGrid((IClientHeightLeafSubGrid) SubGrid, DesignElevations);

      return true;
    }

    /// <summary>
    /// Calculates a cut/fill sub grid from two elevation sub grids, replacing the elevations
    /// in the first sub grid with the resulting cut fill values
    /// </summary>
    /// <param name="subGrid1"></param>
    /// <param name="subGrid2"></param>
    public static void ComputeCutFillSubGrid(IClientHeightLeafSubGrid subGrid1, IClientHeightLeafSubGrid subGrid2)
    {
      SubGridUtilities.SubGridDimensionalIterator((I, J) =>
      {
        if (subGrid1.Cells[I, J] != Consts.NullHeight)
        {
          if (subGrid2.Cells[I, J] != Consts.NullHeight)
            subGrid1.Cells[I, J] = subGrid1.Cells[I, J] - subGrid2.Cells[I, J];
          else
            subGrid1.Cells[I, J] = Consts.NullHeight;
        }
      });
    } 
  }
}
