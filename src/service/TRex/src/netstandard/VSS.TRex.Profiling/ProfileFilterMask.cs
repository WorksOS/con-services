using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// Contains core logic for determining masks for applying to subgrids to effect various restrictions
  /// imposed on the request by a filter
  /// </summary>
  public static class ProfileFilterMask
  {
    private static ILogger Log = Logging.Logger.CreateLogger("ProfileFilterMask");

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
          Intercepts.Items[InterceptIdx].MidPointY, out uint OTGCellX, out uint OTGCellY);

        SubGridCellAddress ThisSubgridOrigin = new SubGridCellAddress(OTGCellX >> SubGridTreeConsts.SubGridIndexBitsPerLevel, OTGCellY >> SubGridTreeConsts.SubGridIndexBitsPerLevel);

        if (!currentSubGridOrigin.Equals(ThisSubgridOrigin))
          break;

        uint CellX = OTGCellX & SubGridTreeConsts.SubGridLocalKeyMask;
          uint CellY = OTGCellY & SubGridTreeConsts.SubGridLocalKeyMask;

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
    /// <param name="currentSubGridOrigin"></param>
    /// <param name="intercepts"></param>
    /// <param name="fromProfileCellIndex"></param>
    /// <param name="mask"></param>
    /// <param name="cellFilter"></param>
    /// <param name="tree"></param>
    /// <param name="surfaceDesignMaskDesign"></param>
    /// <returns></returns>
    public static bool ConstructSubgridCellFilterMask(SubGridCellAddress currentSubGridOrigin,
      InterceptList intercepts,
      int fromProfileCellIndex,
      SubGridTreeBitmapSubGridBits mask,
      ICellSpatialFilter cellFilter,
      ISubGridTree tree,
      IDesign surfaceDesignMaskDesign)
    {
      ConstructSubgridSpatialAndPositionalMask(currentSubGridOrigin, intercepts, fromProfileCellIndex, mask, cellFilter, tree);

      // If the filter contains an alignment design mask filter then compute this and AND it with the
      // mask calculated in the step above to derive the final required filter mask

      if (cellFilter.HasAlignmentDesignMask())
      {
          if (cellFilter.AlignmentFence.IsNull()) // Should have been done in ASNode but if not
            throw new ArgumentException($"Spatial filter does not contained pre-prepared alignment fence for design {cellFilter.AlignmentDesignMaskDesignUID}");

          // Go over set bits and determine if they are in Design fence boundary
          mask.ForEachSetBit((X, Y) =>
          {
            tree.GetCellCenterPosition((uint)(currentSubGridOrigin.X + X), (uint)(currentSubGridOrigin.Y + Y), out var CX, out var CY);
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
        surfaceDesignMaskDesign.GetFilterMask(tree.ID, currentSubGridOrigin, tree.CellSize, out SubGridTreeBitmapSubGridBits filterMask, out DesignProfilerRequestResult requestResult);

        if (requestResult == DesignProfilerRequestResult.OK)
          mask.AndWith(filterMask);
        else
        {
          Log.LogError($"Call (A2) to {nameof(ConstructSubgridCellFilterMask)} returned error result {requestResult} for {cellFilter.SurfaceDesignMaskDesignUid}");
          return false;
        }
      }

      return true;
    }
  }
}
