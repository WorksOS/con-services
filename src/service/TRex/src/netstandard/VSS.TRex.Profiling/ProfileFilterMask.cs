using System;
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
    private static void ConstructSubGridSpatialAndPositionalMask(SubGridCellAddress currentSubGridOrigin,
      InterceptList intercepts,
      int fromProfileCellIndex,
      SubGridTreeBitmapSubGridBits mask,
      ICellSpatialFilter cellFilter,
      ISubGridTree subGridTree)
    {
      var cellFilterHasSpatialOrPositionalFilters = cellFilter.HasSpatialOrPositionalFilters;
      var interceptsCount = intercepts.Count;

      mask.Clear();

      for (var interceptIdx = fromProfileCellIndex; interceptIdx < interceptsCount; interceptIdx++)
      {
        // Determine the on-the-ground cell underneath the midpoint of each cell on the intercept line
        subGridTree.CalculateIndexOfCellContainingPosition(intercepts.Items[interceptIdx].MidPointX,
          intercepts.Items[interceptIdx].MidPointY, out var otgCellX, out var otgCellY);

        var thisSubGridOrigin = new SubGridCellAddress(otgCellX & ~SubGridTreeConsts.SubGridLocalKeyMask, otgCellY & ~SubGridTreeConsts.SubGridLocalKeyMask);

        if (!currentSubGridOrigin.Equals(thisSubGridOrigin))
          break;

        var cellX = otgCellX & SubGridTreeConsts.SubGridLocalKeyMask;
        var cellY = otgCellY & SubGridTreeConsts.SubGridLocalKeyMask;

        if (cellFilterHasSpatialOrPositionalFilters)
        {
          subGridTree.GetCellCenterPosition(otgCellX, otgCellY, out var cellCenterX, out var cellCenterY);

          if (cellFilter.IsCellInSelection(cellCenterX, cellCenterY))
            mask.SetBit(cellX, cellY);
        }
        else
          mask.SetBit(cellX, cellY);
      }
    }

    /// <summary>
    /// Constructs a mask using all spatial filtering elements active in the supplied filter
    /// </summary>
    public static bool ConstructSubGridCellFilterMask(ISiteModel siteModel, SubGridCellAddress currentSubGridOrigin,
      InterceptList intercepts,
      int fromProfileCellIndex,
      SubGridTreeBitmapSubGridBits mask,
      ICellSpatialFilter cellFilter,
      IDesign surfaceDesignMaskDesign)
    {
      ConstructSubGridSpatialAndPositionalMask(currentSubGridOrigin, intercepts, fromProfileCellIndex, mask, cellFilter, siteModel.Grid);

      // If the filter contains an alignment design mask filter then compute this and AND it with the
      // mask calculated in the step above to derive the final required filter mask

      if (cellFilter.HasAlignmentDesignMask())
      {
          if (cellFilter.AlignmentFence.IsNull()) // Should have been done in ASNode but if not
            throw new ArgumentException($"Spatial filter does not contained pre-prepared alignment fence for design {cellFilter.AlignmentDesignMaskDesignUID}");

          var tree = siteModel.Grid;
          // Go over set bits and determine if they are in Design fence boundary
          mask.ForEachSetBit((x, y) =>
          {
            tree.GetCellCenterPosition(currentSubGridOrigin.X + x, currentSubGridOrigin.Y + y, out var cx, out var cy);
            if (!cellFilter.AlignmentFence.IncludesPoint(cx, cy))
            {
              mask.ClearBit(x, y); // remove interest as its not in design boundary
            }
          });
      }

      // If the filter contains a design mask filter then compute this and AND it with the
      // mask calculated in the step above to derive the final required filter mask

      if (surfaceDesignMaskDesign != null)
      {
        var getFilterMaskResult = surfaceDesignMaskDesign.GetFilterMaskViaLocalCompute(siteModel, currentSubGridOrigin, siteModel.CellSize);

        if (getFilterMaskResult.errorCode == DesignProfilerRequestResult.OK)
        {
          if (getFilterMaskResult.filterMask == null)
          {
            _log.LogError("FilterMask null in response from surfaceDesignMaskDesign.GetFilterMask, ignoring it's contribution to filter mask");
          }
          else
          {
            mask.AndWith(getFilterMaskResult.filterMask);
          }
        }
        else
        {
          _log.LogError($"Call (A2) to {nameof(ConstructSubGridCellFilterMask)} returned error result {getFilterMaskResult.errorCode} for {cellFilter.SurfaceDesignMaskDesignUid}");
          return false;
        }
      }

      return true;
    }
  }
}
