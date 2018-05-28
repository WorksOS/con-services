using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.DesignProfiling;
using VSS.TRex.Designs.Storage;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Profiling
{
  public static class LiftFilterMask
  {
    private static ILogger Log = Logging.Logger.CreateLogger("LiftFilterMask");

    public static void ConstructSubgridSpatialAndPositionalMask(ISubGridTree tree, ISubGrid subGrid,
      SubGridCellAddress currentSubGridOrigin, List<ProfileCell> profileCells, ref SubGridTreeBitmapSubGridBits mask,
      int fromProfileCellIndex, CellSpatialFilter cellFilter)
    {
      mask.Clear();

      //with CellFilter, FSubGridTree do
      // from current position to end
      for (int CellIdx = fromProfileCellIndex; CellIdx < profileCells.Count; CellIdx++)
      {
        SubGridCellAddress ThisSubgridOrigin = new SubGridCellAddress(
          profileCells[CellIdx].OTGCellX >> SubGridTree.SubGridIndexBitsPerLevel,
          profileCells[CellIdx].OTGCellY >> SubGridTree.SubGridIndexBitsPerLevel);

        if (!currentSubGridOrigin.Equals(ThisSubgridOrigin))
          break;

        subGrid.GetSubGridCellIndex(profileCells[CellIdx].OTGCellX, profileCells[CellIdx].OTGCellY, out byte CellX,
          out byte CellY);
        if (cellFilter.HasSpatialOrPostionalFilters)
        {
          tree.GetCellCenterPosition(profileCells[CellIdx].OTGCellX, profileCells[CellIdx].OTGCellY,
            out double CellCenterX, out double CellCenterY);
          if (cellFilter.IsCellInSelection(CellCenterX, CellCenterY))
            mask.SetBit(CellX, CellY);
        }
        else
          mask.SetBit(CellX, CellY);
      }
    }

    public static bool ConstructSubgridCellFilterMask(ISubGridTree tree, ISubGrid subGrid,
      SubGridCellAddress currentSubGridOrigin, List<ProfileCell> profileCells, ref SubGridTreeBitmapSubGridBits mask,
      int fromProfileCellIndex, CellSpatialFilter cellFilter)
    {
      //double OriginX, OriginY;

      //      SubGridTreeBitmapSubGridBits DesignMask;
      //      SubGridTreeBitmapSubGridBits DesignFilterMask;
      //      DesignProfilerRequestResult RequestResult;
      // bool Result;

      ConstructSubgridSpatialAndPositionalMask(tree, subGrid, currentSubGridOrigin, profileCells, ref mask,
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

    public static bool InitialiseFilterContext(ISiteModel siteModel, CellPassAttributeFilter passFilter, ProfileCell profileCell, Design design)
    {
      if (passFilter.HasElevationRangeFilter)
      {
        // If the elevation range filter uses a design then the design elevations
        // for the subgrid need to be calculated and supplied to the filter

        if (passFilter.ElevationRangeDesignID != Guid.Empty)
        {
          design.GetDesignHeights(siteModel.ID, new SubGridCellAddress(profileCell.OTGCellX, profileCell.OTGCellY),
            siteModel.Grid.CellSize, out ClientHeightLeafSubGrid FilterDesignElevations,
            out DesignProfilerRequestResult FilterDesignErrorCode);

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
