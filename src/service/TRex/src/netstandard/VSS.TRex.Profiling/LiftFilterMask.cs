using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// Provides support for determining inclusion masks for subgrid cell selection and processing based on spatial, positional
  /// and design based spatial selection criteria from filters
  /// </summary>
  public static class LiftFilterMask<T> where T : class, IProfileCellBase
  {
    private static ILogger Log = Logging.Logger.CreateLogger("LiftFilterMask");

    private static void ConstructSubgridSpatialAndPositionalMask(ISubGridTree tree, 
      SubGridCellAddress currentSubGridOrigin, 
      List<T> profileCells, 
      SubGridTreeBitmapSubGridBits mask,
      int fromProfileCellIndex, 
      ICellSpatialFilter cellFilter)
    {
      mask.Clear();

      // From current position to end...
      for (int CellIdx = fromProfileCellIndex; CellIdx < profileCells.Count; CellIdx++)
      {
        T profileCell = profileCells[CellIdx];
        SubGridCellAddress ThisSubgridOrigin = new SubGridCellAddress(
          profileCell.OTGCellX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
          profileCell.OTGCellY >> SubGridTreeConsts.SubGridIndexBitsPerLevel);

        if (!currentSubGridOrigin.Equals(ThisSubgridOrigin))
          break;

        byte CellX = (byte)(profileCell.OTGCellX & SubGridTreeConsts.SubGridLocalKeyMask);
        byte CellY = (byte)(profileCell.OTGCellY & SubGridTreeConsts.SubGridLocalKeyMask);

        if (cellFilter.HasSpatialOrPositionalFilters)
        {
          tree.GetCellCenterPosition(profileCell.OTGCellX, profileCell.OTGCellY,
            out double CellCenterX, out double CellCenterY);
          if (cellFilter.IsCellInSelection(CellCenterX, CellCenterY))
            mask.SetBit(CellX, CellY);
        }
        else
          mask.SetBit(CellX, CellY);
      }
    }

    public static bool ConstructSubgridCellFilterMask(ISubGridTree tree, 
      SubGridCellAddress currentSubGridOrigin, 
      List<T> profileCells,
      SubGridTreeBitmapSubGridBits mask,
      int fromProfileCellIndex,
      ICellSpatialFilter cellFilter,
      IDesign SurfaceDesignMaskDesign)
    {
      // double OriginX, OriginY;

      ConstructSubgridSpatialAndPositionalMask(tree, currentSubGridOrigin, profileCells, mask, fromProfileCellIndex, cellFilter);

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

      if (SurfaceDesignMaskDesign != null)
      {
        SurfaceDesignMaskDesign.GetFilterMask(tree.ID, currentSubGridOrigin, tree.CellSize, out SubGridTreeBitmapSubGridBits filterMask, out DesignProfilerRequestResult requestResult);

        if (requestResult == DesignProfilerRequestResult.OK)
          mask.AndWith(filterMask);
        else
        {
          Log.LogError($"Call (B2) to {nameof(ConstructSubgridCellFilterMask)} returned error result {requestResult} for {cellFilter.SurfaceDesignMaskDesignUid}");
          return false;
        }
      }

      return true;
    }

    public static bool InitialiseFilterContext(ISiteModel siteModel, ICellPassAttributeFilter passFilter, ProfileCell profileCell, IDesign design, out DesignProfilerRequestResult FilterDesignErrorCode)
    {
      FilterDesignErrorCode = DesignProfilerRequestResult.UnknownError;

      if (passFilter.HasElevationRangeFilter)
      {
        // If the elevation range filter uses a design then the design elevations
        // for the subgrid need to be calculated and supplied to the filter

        if (passFilter.ElevationRangeDesignUID != Guid.Empty)
        {
          design.GetDesignHeights(siteModel.ID, new SubGridCellAddress(profileCell.OTGCellX, profileCell.OTGCellY),
            siteModel.Grid.CellSize, out IClientHeightLeafSubGrid FilterDesignElevations, out FilterDesignErrorCode);

          if (FilterDesignErrorCode != DesignProfilerRequestResult.OK || FilterDesignElevations == null)
          {
            if (FilterDesignErrorCode == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
              Log.LogInformation(
                "Lift filter by design. Call to RequestDesignElevationPatch failed due to no elevations in requested patch.");
            else
              Log.LogWarning(
                $"Lift filter by design. Call to RequestDesignElevationPatch failed due to no TDesignProfilerRequestResult return code {FilterDesignErrorCode}.");
            return false;
          }

          passFilter.InitialiseElevationRangeFilter(FilterDesignElevations);
        }
      }
      return true;
    }
  }
}
