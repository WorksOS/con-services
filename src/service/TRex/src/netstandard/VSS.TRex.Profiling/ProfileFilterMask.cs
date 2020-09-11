using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// Contains core logic for determining masks for applying to sub grids to effect various restrictions
  /// imposed on the request by a filter
  /// </summary>
  public static class ProfileFilterMask
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger("ProfileFilterMask");

    /// <summary>
    /// Constructs a mask using polygonal and positional spatial filtering aspects of a filter.
    /// </summary>
    /// <param name="currentSubGridOrigin"></param>
    /// <param name="Intercepts"></param>
    /// <param name="fromProfileCellIndex"></param>
    /// <param name="Mask"></param>
    /// <param name="cellFilter"></param>
    /// <param name="SubGridTree"></param>
    private static void ConstructSubgridSpatialAndPositionalMask(SubGridCellAddress currentSubGridOrigin,
      InterceptList Intercepts,
      int fromProfileCellIndex,
      SubGridTreeBitmapSubGridBits Mask,
      ICellSpatialFilter cellFilter,
      ISubGridTree SubGridTree)
    {
      bool cellFilter_HasSpatialOrPositionalFilters = cellFilter.HasSpatialOrPositionalFilters;
      int Intercepts_Count = Intercepts.Count;

      Mask.Clear();

      for (int InterceptIdx = fromProfileCellIndex; InterceptIdx < Intercepts_Count; InterceptIdx++)
      {
        // Determine the on-the-ground cell underneath the midpoint of each cell on the intercept line
        SubGridTree.CalculateIndexOfCellContainingPosition(Intercepts.Items[InterceptIdx].MidPointX,
          Intercepts.Items[InterceptIdx].MidPointY, out int OTGCellX, out int OTGCellY);

        SubGridCellAddress ThisSubgridOrigin = new SubGridCellAddress(OTGCellX & ~SubGridTreeConsts.SubGridLocalKeyMask, OTGCellY & ~SubGridTreeConsts.SubGridLocalKeyMask);

        if (!currentSubGridOrigin.Equals(ThisSubgridOrigin))
          break;

        int CellX = OTGCellX & SubGridTreeConsts.SubGridLocalKeyMask;
          int CellY = OTGCellY & SubGridTreeConsts.SubGridLocalKeyMask;

          if (cellFilter_HasSpatialOrPositionalFilters)
          {
            SubGridTree.GetCellCenterPosition(OTGCellX, OTGCellY, out double CellCenterX, out double CellCenterY);

            if (cellFilter.IsCellInSelection(CellCenterX, CellCenterY))
              Mask.SetBit(CellX, CellY);
          }
          else
            Mask.SetBit(CellX, CellY);
      }
    }

    /// <summary>
    /// Constructs a mask using all spatial filtering elements active in the supplied filter
    /// </summary>
    public static bool ConstructSubgridCellFilterMask(ISiteModel siteModel, SubGridCellAddress currentSubGridOrigin,
      InterceptList intercepts,
      int fromProfileCellIndex,
      SubGridTreeBitmapSubGridBits mask,
      ICellSpatialFilter cellFilter,
      IDesign surfaceDesignMaskDesign)
    {
      ConstructSubgridSpatialAndPositionalMask(currentSubGridOrigin, intercepts, fromProfileCellIndex, mask, cellFilter, siteModel.Grid);

      // If the filter contains an alignment design mask filter then compute this and AND it with the
      // mask calculated in the step above to derive the final required filter mask

      if (cellFilter.HasAlignmentDesignMask())
      {
          if (cellFilter.AlignmentFence.IsNull()) // Should have been done in ASNode but if not
            throw new ArgumentException($"Spatial filter does not contained pre-prepared alignment fence for design {cellFilter.AlignmentDesignMaskDesignUID}");

          var tree = siteModel.Grid;
          // Go over set bits and determine if they are in Design fence boundary
          mask.ForEachSetBit((X, Y) =>
          {
            tree.GetCellCenterPosition(currentSubGridOrigin.X + X, currentSubGridOrigin.Y + Y, out var CX, out var CY);
            if (!cellFilter.AlignmentFence.IncludesPoint(CX, CY))
            {
              mask.ClearBit(X, Y); // remove interest as its not in design boundary
            }
          });
      }

      // If the filter contains a design mask filter then compute this and AND it with the
      // mask calculated in the step above to derive the final required filter mask

      if (surfaceDesignMaskDesign != null)
      {
        var getFilterMaskResult = surfaceDesignMaskDesign.GetFilterMaskViaLocalCompute(siteModel, currentSubGridOrigin, siteModel.CellSize);

        if (getFilterMaskResult.errorCode == DesignProfilerRequestResult.OK || getFilterMaskResult.errorCode == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
        {
          if (getFilterMaskResult.filterMask == null)
          {
            _log.LogWarning("FilterMask null in response from surfaceDesignMaskDesign.GetFilterMask, ignoring it's contribution to filter mask");
          }
          else
          {
            mask.AndWith(getFilterMaskResult.filterMask);
          }
        }
        else
        {
          _log.LogError($"Call (A2) to {nameof(ConstructSubgridCellFilterMask)} returned error result {getFilterMaskResult.errorCode} for {cellFilter.SurfaceDesignMaskDesignUid}");
          return false;
        }
      }

      return true;
    }
  }
}
