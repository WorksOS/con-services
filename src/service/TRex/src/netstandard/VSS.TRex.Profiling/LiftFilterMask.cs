﻿using System;
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
  public static class LiftFilterMask
  {
    private static ILogger Log = Logging.Logger.CreateLogger("LiftFilterMask");

    public static void ConstructSubgridSpatialAndPositionalMask(ISubGridTree tree,
      SubGridCellAddress currentSubGridOrigin, List<IProfileCell> profileCells, SubGridTreeBitmapSubGridBits mask,
      int fromProfileCellIndex, ICellSpatialFilter cellFilter)
    {
      mask.Clear();

      //with CellFilter, FSubGridTree do
      // from current position to end
      for (int CellIdx = fromProfileCellIndex; CellIdx < profileCells.Count; CellIdx++)
      {
        ProfileCell profileCell = (ProfileCell)profileCells[CellIdx];
        SubGridCellAddress ThisSubgridOrigin = new SubGridCellAddress(
          profileCell.OTGCellX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
          profileCell.OTGCellY >> SubGridTreeConsts.SubGridIndexBitsPerLevel);

        if (!currentSubGridOrigin.Equals(ThisSubgridOrigin))
          break;

        byte CellX = (byte)(profileCell.OTGCellX & SubGridTreeConsts.SubGridLocalKeyMask);
        byte CellY = (byte)(profileCell.OTGCellY & SubGridTreeConsts.SubGridLocalKeyMask);

        if (cellFilter.HasSpatialOrPostionalFilters)
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
      SubGridCellAddress currentSubGridOrigin, List<IProfileCell> profileCells, SubGridTreeBitmapSubGridBits mask,
      int fromProfileCellIndex, ICellSpatialFilter cellFilter)
    {
      // double OriginX, OriginY;

      // SubGridTreeBitmapSubGridBits DesignMask;
      // SubGridTreeBitmapSubGridBits DesignFilterMask;
      //      DesignProfilerRequestResult RequestResult;
      // bool Result;

      ConstructSubgridSpatialAndPositionalMask(tree, currentSubGridOrigin, profileCells, mask,
        fromProfileCellIndex, cellFilter);

      // If the filter contains a design mask filter then compute this and AND it with the
      // mask calculated in the step above to derive the final required filter mask

      if (cellFilter.HasAlignmentDesignMask())
      {
        /* TODO: Alignment design mask not yet supported 

  // Query the design profiler service for the corresponding filter mask given the
  // alignment design configured in the cell selection filter.

  CompositeHeightsGrid.CalculateWorldOrigin(OriginX, OriginY);
  with DesignProfilerLayerLoadBalancer.LoadBalancedDesignProfilerService do
    {
      RequestResult := RequestDesignMaskFilterPatch(Construct_ComputeDesignFilterPatch_Args(FSiteModel.ID,
                                                                                            OriginX, OriginY,
                                                                                            FSiteModel.Grid.CellSize,
                                                                                            ReferenceDesign,
                                                                                            Mask,
                                                                                            StartStation, EndStation,
                                                                                            LeftOffset, RightOffset),
                                                                                            DesignMask);

      if RequestResult = dppiOK then
        Mask := Mask AND DesignMask
      else
        {
          Result := False;
          SIGLogMessage.PublishNoODS(Nil, Format('Call(B1) to RequestDesignMaskFilterPatch in TICServerProfiler returned error result %s for %s.',
                                                 [DesignProfilerErrorStatusName(RequestResult), CellFilter.ReferenceDesign.ToString]), slmcError);
        }
    }
    */
      }

      if (cellFilter.HasAlignmentDesignMask())
      {
        /* todo Design elevation requests not yet supported
    
          // Query the design profiler service for the corresponding filter mask given the
          // tin design configured in the cell selection filter.
    
          with DesignProfilerLayerLoadBalancer.LoadBalancedDesignProfilerService do
            {
              RequestResult := RequestDesignMaskFilterPatch(Construct_ComputeDesignFilterPatch_Args(FSiteModel.ID,
                                                                                                    OriginX, OriginY,
                                                                                                    FSiteModel.Grid.CellSize,
                                                                                                    DesignFilter,
                                                                                                    Mask,
                                                                                                    StartStation, EndStation,
                                                                                                    LeftOffset, RightOffset),
                                                            DesignFilterMask);
    
              if RequestResult = dppiOK then
                Mask := Mask AND DesignFilterMask
              else
                {
                  Result := False;
                  SIGLogMessage.PublishNoODS(Nil, Format('Call (B2) to RequestDesignMaskFilterPatch in TICServerProfiler returned error result %s for %s.',
                                                         [DesignProfilerErrorStatusName(RequestResult), CellFilter.DesignFilter.ToString]), slmcError);
                }
            }
        */
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

        if (passFilter.ElevationRangeDesignID != Guid.Empty)
        {
          design.GetDesignHeights(siteModel.ID, new SubGridCellAddress(profileCell.OTGCellX, profileCell.OTGCellY),
            siteModel.Grid.CellSize, out IClientHeightLeafSubGrid FilterDesignElevations,
            out FilterDesignErrorCode);

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
