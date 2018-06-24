using VSS.TRex.Filters;
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
    /// <summary>
    /// Constructs a mask using polygonal and positional spatial filtering aspects of a fitler.
    /// </summary>
    /// <param name="currentSubGridOrigin"></param>
    /// <param name="Intercepts"></param>
    /// <param name="fromProfileCellIndex"></param>
    /// <param name="Mask"></param>
    /// <param name="cellFilter"></param>
    /// <param name="SubGridTree"></param>
    public static void ConstructSubgridSpatialAndPositionalMask(SubGridCellAddress currentSubGridOrigin,
      InterceptList Intercepts,
      int fromProfileCellIndex,
      ref SubGridTreeBitmapSubGridBits Mask,
      CellSpatialFilter cellFilter,
      ISubGridTree SubGridTree)
    {
      bool cellFilter_HasSpatialOrPostionalFilters = cellFilter.HasSpatialOrPostionalFilters;
      int Intercepts_Count = Intercepts.Count;

      Mask.Clear();

      for (int InterceptIdx = fromProfileCellIndex; InterceptIdx < Intercepts_Count; InterceptIdx++)
      {
        // Determine the on-the-ground cell underneath the midpoint of each cell on the intercept line
        SubGridTree.CalculateIndexOfCellContainingPosition(Intercepts.Items[InterceptIdx].MidPointX,
          Intercepts.Items[InterceptIdx].MidPointY, out uint OTGCellX, out uint OTGCellY);

        SubGridCellAddress ThisSubgridOrigin = new SubGridCellAddress(OTGCellX >> TRex.SubGridTree.SubGridIndexBitsPerLevel, OTGCellY >> TRex.SubGridTree.SubGridIndexBitsPerLevel);

        if (!currentSubGridOrigin.Equals(ThisSubgridOrigin))
          break;

        uint CellX = OTGCellX & TRex.SubGridTree.SubGridLocalKeyMask;
          uint CellY = OTGCellY & TRex.SubGridTree.SubGridLocalKeyMask;

          if (cellFilter_HasSpatialOrPostionalFilters)
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
    /// Constructs a mask using all spatial fitlering elements active in the supplied filter
    /// </summary>
    /// <param name="currentSubGridOrigin"></param>
    /// <param name="Intercepts"></param>
    /// <param name="fromProfileCellIndex"></param>
    /// <param name="Mask"></param>
    /// <param name="cellFilter"></param>
    /// <param name="SubGridTree"></param>
    /// <returns></returns>
    public static bool ConstructSubgridCellFilterMask(SubGridCellAddress currentSubGridOrigin,
      InterceptList Intercepts,
      int fromProfileCellIndex,
      ref SubGridTreeBitmapSubGridBits Mask,
      CellSpatialFilter cellFilter,
      ISubGridTree SubGridTree)
    {
      //      SubGridTreeBitmapSubGridBits DesignMask;
      //      SubGridTreeBitmapSubGridBits DesignFilterMask;
      //      DesignProfilerRequestResult RequestResult;
      //      bool Result = true;

      ConstructSubgridSpatialAndPositionalMask(currentSubGridOrigin, Intercepts, fromProfileCellIndex, ref Mask,
        cellFilter, SubGridTree);

      // If the filter contains a design mask filter then compute this and AND it with the
      // mask calculated in the step above to derive the final required filter mask

      if (cellFilter.HasAlignmentDesignMask())
      {
        // Query the design profiler service for the corresponding filter mask given the
        // alignment design configured in the cell selection filter.
        /* TODO: Alignment design mask not yet supported 
        with DesignProfilerLayerLoadBalancer.LoadBalancedDesignProfilerService do
        {
          RequestResult = RequestDesignMaskFilterPatch(Construct_ComputeDesignFilterPatch_Args(SiteModel.ID,
              SubgridOriginX, SubgridOriginY,
              SiteModel.Grid.CellSize,
              ReferenceDesign,
              Mask,
              StartStation, EndStation,
              LeftOffset, RightOffset),
            DesignMask);

          if (RequestResult == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
            Mask = Mask & DesignMask;
          else
          {
            Result = false;
            //SIGLogMessage.PublishNoODS(Nil, Format('Call (A1) to RequestDesignMaskFilterPatch in TICServerProfiler returned error result %s for %s.', 
            //    [DesignProfilerErrorStatusName(RequestResult), CellFilter.ReferenceDesign.ToString]), slmcError);
          }
        }
        */
      }

      if (cellFilter.HasAlignmentDesignMask())
      {
        // Query the design profiler service for the corresponding filter mask given the
        // alignment design configured in the cell selection filter.

        /* todo Design elevation requests not yet supported
        with DesignProfilerLayerLoadBalancer.LoadBalancedDesignProfilerService do
        {
          RequestResult = RequestDesignMaskFilterPatch(Construct_ComputeDesignFilterPatch_Args(SiteModel.ID,
              SubgridOriginX, SubgridOriginY,
              SiteModel.Grid.CellSize,
              DesignFilter,
              Mask,
              StartStation, EndStation,
              LeftOffset, RightOffset),
            DesignFilterMask);

          if (RequestResult == DesignProfilerRequestResult.OK)
            Mask = Mask & DesignFilterMask;
          else
          {
            Result = false;
            //SIGLogMessage.PublishNoODS(Nil, Format('Call (A2) to RequestDesignMaskFilterPatch in TICServerProfiler returned error result %s for %s.', 
            //    [DesignProfilerErrorStatusName(RequestResult), CellFilter.DesignFilter.ToString]), slmcError);
          }
        }
        */
      }

      return true;
    }
  }
}
