using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Types;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// Responsible for orchestrating analysis of identified cells along the path of a profile line
  /// and deriving the profile related analytics for each cell
  /// </summary>
  public class SummaryVolumesCellProfileAnalyzer : CellProfileAnalyzerBase<SummaryVolumeProfileCell>
  {
    private static ILogger Log = Logging.Logger.CreateLogger<CellProfileAnalyzer>();

    private SummaryVolumeProfileCell profileCell;

    private SummaryVolumeProfileCell[] profileCellList = new SummaryVolumeProfileCell[100];

    public VolumeComputationType VolumeType { get; set; } = VolumeComputationType.None;

    private ISubGridRequestor[] Requestors;

    private bool IntermediaryFilterRequired = false;

    private IDesign svDesign;

    private int cellCounter;

    private SummaryVolumesCellProfileAnalyzer()
    {}

    /// <summary>
    /// Constructs a profile lift builder that analyzes cells in a cell profile vector
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="pDExistenceMap"></param>
    /// <param name="filterSet"></param>
    /// <param name="cellPassFilter_ElevationRangeDesign"></param>
    /// <param name="referenceDesign"></param>
    /// <param name="cellLiftBuilder"></param>
    public SummaryVolumesCellProfileAnalyzer(ISiteModel siteModel,
      ISubGridTreeBitMask pDExistenceMap,
      IFilterSet filterSet,
      IDesign cellPassFilter_ElevationRangeDesign,
      IDesign referenceDesign,
      ICellLiftBuilder cellLiftBuilder) : base(siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesign)
    {
      svDesign = referenceDesign;
    }

    /// <summary>
    /// Constructs the set of filters that will be used to derive the set of production data subgrids required for
    /// each subgrid being considered along the profile line.
    /// </summary>
    /// <returns></returns>
    private IFilterSet ConstructFilters()
    {
      // If the volume calculation is between two filters then handle appropriately...
      if (VolumeType == VolumeComputationType.Between2Filters)
      {
        var BaseFilter = FilterSet.Filters[0];
        var TopFilter = FilterSet.Filters[1];

        // Determine if intermediary filter/surface behaviour is required to support summary volumes
        IntermediaryFilterRequired = VolumeType == VolumeComputationType.Between2Filters &&
                                          BaseFilter.AttributeFilter.HasTimeFilter && BaseFilter.AttributeFilter.StartTime == DateTime.MinValue && // 'From' has As-At Time filter
                                          !BaseFilter.AttributeFilter.ReturnEarliestFilteredCellPass && // Want latest cell pass in 'from'
                                          TopFilter.AttributeFilter.HasTimeFilter && TopFilter.AttributeFilter.StartTime != DateTime.MinValue && // 'To' has time-range filter with latest
                                          !TopFilter.AttributeFilter.ReturnEarliestFilteredCellPass; // Want latest cell pass in 'to'

        if (IntermediaryFilterRequired)
        {
          // Create and use the intermediary filter. The intermediary filter
          // is create from the Top filter, with the return earliest flag set to true
          var IntermediaryFilter = new CombinedFilter();
          IntermediaryFilter.AttributeFilter.Assign(TopFilter.AttributeFilter);
          IntermediaryFilter.AttributeFilter.ReturnEarliestFilteredCellPass = true;

          return new FilterSet(new[] {FilterSet.Filters[0], IntermediaryFilter, FilterSet.Filters[1]});
        }
      }

      return FilterSet;
    }

    /// <summary>
    /// Merges the 'from' elevation subgrid and the 'intermediary' subgrid result into a single subgrid for 
    /// subsequent calculation. THe result is placed into the 'from' subgrid.
    /// </summary>
    private void MergeIntemediaryResults(ClientHeightAndTimeLeafSubGrid heightGrid1, ClientHeightAndTimeLeafSubGrid intermediaryHeightGrid)
    {
      // Combine this result with the result of the first query to obtain a modified heights grid

      // Merge the first two results to give the profile calc the correct combined 'from' surface
      // HeightsGrid1 is 'latest @ first filter', HeightsGrid1 is earliest @ second filter
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        if (heightGrid1.Cells[x, y] == CellPassConsts.NullHeight &&
            // Check if there is a non null candidate in the earlier @ second filter
            intermediaryHeightGrid.Cells[x, y] != CellPassConsts.NullHeight)
        {
          heightGrid1.Cells[x, y] = intermediaryHeightGrid.Cells[x, y];
          heightGrid1.Times[x, y] = intermediaryHeightGrid.Times[x, y];
        }
      });
    }

    /// <summary>
    /// Processes each subgrid in turn into the resulting profile.
    /// </summary>
    public void ProcessSubGroup(SubGridCellAddress address, bool prodDataAtAddress, SubGridTreeBitmapSubGridBits cellOverrideMask)
    { 
      bool noErrors = true;

      // Execute a client grid request for each requester and create an array of the results
      var clientGrids = Requestors.Select(x =>
      {

        var clientGrid = ClientLeafSubGridFactory.GetSubGridEx(GridDataType.HeightAndTime, SiteModel.Grid.CellSize, SubGridTreeConsts.SubGridTreeLevels,
          (uint)(address.X & ~SubGridTreeConsts.SubGridLocalKeyMask), (uint)(address.Y & ~SubGridTreeConsts.SubGridLocalKeyMask));

        x.CellOverrideMask = cellOverrideMask;

        // Reach into the subgrid request layer and retrieve an appropriate subgrid
        ServerRequestResult result = x.RequestSubGridInternal((SubGridCellAddress)address, prodDataAtAddress, true, clientGrid);
        if (result != ServerRequestResult.NoError)
          Log.LogError($"Request for subgrid {address} request failed with code {result}");

        return clientGrid;
      }).ToArray();

      // If an intermediary result was requested then merge the 'from' and intermediary subgrids now
      if (IntermediaryFilterRequired)
      {
        MergeIntemediaryResults(clientGrids[0] as ClientHeightAndTimeLeafSubGrid, clientGrids[1] as ClientHeightAndTimeLeafSubGrid);

        //... and chop out the intermediary grid
        clientGrids = new[] {clientGrids[0], clientGrids[2]};
      }

      var heightsGrid1 = clientGrids[0] as ClientHeightAndTimeLeafSubGrid;
      var heightsGrid2 = clientGrids[1] as ClientHeightAndTimeLeafSubGrid;

      IClientHeightLeafSubGrid designHeights = null;

      if (VolumeType == VolumeComputationType.BetweenFilterAndDesign || VolumeType == VolumeComputationType.BetweenDesignAndFilter)
      {


        if (svDesign != null)
        {

          svDesign.GetDesignHeights(SiteModel.ID, address, SiteModel.Grid.CellSize, out designHeights, out var errorCode);

          if (errorCode != DesignProfilerRequestResult.OK || designHeights == null)
          {
            string errorMessage;
            noErrors = false;
            if (errorCode == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
            {
              errorMessage = "Call to RequestDesignElevationPatch failed due to no elevations in requested patch.";
              Log.LogInformation(errorMessage);
            }
            else
            {
              errorMessage = $"Call to RequestDesignElevationPatch failed due to no TDesignProfilerRequestResult return code {errorCode}.";
              Log.LogError(errorMessage);
            }
          }
        }
        else
        {
          Log.LogError("Missing design reference. Call to request Summary Volumes Profile using design failed due to no reference design");
        }

      }

      if (noErrors)
      {
        for (int I = 0; I < cellCounter; I++)
        {
          uint cellX = profileCellList[I].OTGCellX & SubGridTreeConsts.SubGridLocalKeyMask;
          uint cellY = profileCellList[I].OTGCellY & SubGridTreeConsts.SubGridLocalKeyMask;
          if (heightsGrid1 != null)
            profileCellList[I].LastCellPassElevation1 = heightsGrid1.Cells[cellX, cellY];
          if (heightsGrid2 != null)
            profileCellList[I].LastCellPassElevation2 = heightsGrid2.Cells[cellX, cellY];
          profileCellList[I].DesignElev = designHeights?.Cells[cellX, cellY] ?? CellPassConsts.NullHeight;

        }
      }

    }


    /// <summary>
    /// Builds a fully analyzed vector of profiled cells from the list of cell passed to it
    /// </summary>
    /// <param name="profileCells"></param>
    /// <param name="cellPassIterator"></param>
    /// <returns></returns>
    ///
    public override bool Analyze(List<SummaryVolumeProfileCell> profileCells, ISubGridSegmentCellPassIterator cellPassIterator)
    {

      Log.LogDebug($"Analyze Summary Volume ProfileCells. Processing {profileCells.Count}");

      SubGridCellAddress CurrentSubgridOrigin = new SubGridCellAddress(int.MaxValue, int.MaxValue);
      ISubGrid SubGrid = null;
      IServerLeafSubGrid _SubGridAsLeaf = null;
      profileCell = null;

      // Construct the set of requestors to query elevation subgrids needed for the summary volume calculations.
      var utilities = DIContext.Obtain<IRequestorUtilities>();
      Requestors = utilities.ConstructRequestors(SiteModel,
        utilities.ConstructRequestorIntermediaries(SiteModel, ConstructFilters(), true, GridDataType.HeightAndTime),
        AreaControlSet.CreateAreaControlSet(), PDExistenceMap);

      SubGridTreeBitmapSubGridBits cellOverrideMask = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);


      for (int I = 0; I < profileCells.Count; I++)
      {

        profileCell = profileCells[I];

        // get subgrid origin for cell address
        SubGridCellAddress thisSubgridOrigin = new SubGridCellAddress(profileCell.OTGCellX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
                                                   profileCell.OTGCellY >> SubGridTreeConsts.SubGridIndexBitsPerLevel);

        if (!CurrentSubgridOrigin.Equals(thisSubgridOrigin)) // if we have a new subgrid to fetch 
        {
          // if we have an existing subgrid and a change in subgrid detected process the current subgrid profilecell list
          if (SubGrid != null)
          {
            ProcessSubGroup(new SubGridCellAddress(CurrentSubgridOrigin.X << SubGridTreeConsts.SubGridIndexBitsPerLevel, CurrentSubgridOrigin.Y << SubGridTreeConsts.SubGridIndexBitsPerLevel),
                            PDExistenceMap[CurrentSubgridOrigin.X, CurrentSubgridOrigin.Y], cellOverrideMask);
            cellOverrideMask.Clear();
          }

          SubGrid = null;
          cellCounter = 0;

          // Does the subgrid tree contain this node in it's existence map? if so get subgrid
          if (PDExistenceMap[thisSubgridOrigin.X, thisSubgridOrigin.Y])
            SubGrid = SubGridTrees.Server.Utilities.SubGridUtilities.LocateSubGridContaining
              (StorageProxy, SiteModel.Grid, profileCell.OTGCellX, profileCell.OTGCellY, SiteModel.Grid.NumLevels, false, false);

          _SubGridAsLeaf = SubGrid as ServerSubGridTreeLeaf;
          if (_SubGridAsLeaf == null)
            continue;

          CurrentSubgridOrigin = thisSubgridOrigin; // all good to proceed with this subgrid

        }

        profileCellList[cellCounter++] = profileCell; // add cell to list to process for this subgrid
        cellOverrideMask.SetBit(profileCell.OTGCellX & SubGridTreeConsts.SubGridLocalKeyMask, profileCell.OTGCellY & SubGridTreeConsts.SubGridLocalKeyMask);
      }

      if (cellCounter > 0 && SubGrid != null) // Make sure we process last list
      {
        ProcessSubGroup(new SubGridCellAddress(CurrentSubgridOrigin.X << SubGridTreeConsts.SubGridIndexBitsPerLevel, CurrentSubgridOrigin.Y << SubGridTreeConsts.SubGridIndexBitsPerLevel),
                        PDExistenceMap[CurrentSubgridOrigin.X, CurrentSubgridOrigin.Y], cellOverrideMask);
      }

      return true;
    }
  }
}

