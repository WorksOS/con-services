using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
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

    private IDesignWrapper svDesignWrapper;

    private int cellCounter;

    private SummaryVolumesCellProfileAnalyzer()
    {}

    /// <summary>
    /// Constructs a profile lift builder that analyzes cells in a cell profile vector
    /// </summary>
    public SummaryVolumesCellProfileAnalyzer(ISiteModel siteModel,
      ISubGridTreeBitMask pDExistenceMap,
      IFilterSet filterSet,
      IDesignWrapper referenceDesignWrapper,
      ICellLiftBuilder cellLiftBuilder,
      VolumeComputationType volumeType,
      IOverrideParameters overrides,
      ILiftParameters liftParams) 
      : base(siteModel, pDExistenceMap, filterSet, overrides, liftParams)
    {
      svDesignWrapper = referenceDesignWrapper;
      VolumeType = volumeType;
    }

    /// <summary>
    /// Constructs the set of filters that will be used to derive the set of production data sub grids required for
    /// each sub grid being considered along the profile line.
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
                                          BaseFilter.AttributeFilter.HasTimeFilter && BaseFilter.AttributeFilter.StartTime == Consts.MIN_DATETIME_AS_UTC && // 'From' has As-At Time filter
                                          !BaseFilter.AttributeFilter.ReturnEarliestFilteredCellPass && // Want latest cell pass in 'from'
                                          TopFilter.AttributeFilter.HasTimeFilter && TopFilter.AttributeFilter.StartTime != Consts.MIN_DATETIME_AS_UTC && // 'To' has time-range filter with latest
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

      if (VolumeType == VolumeComputationType.BetweenDesignAndFilter)
        return new FilterSet(new [] { FilterSet.Filters[1] });

      if (VolumeType == VolumeComputationType.BetweenFilterAndDesign)
        return new FilterSet(new[] { FilterSet.Filters[0] });

      return FilterSet;
    }

    /// <summary>
    /// Merges the 'from' elevation sub grid and the 'intermediary' sub grid result into a single sub grid for 
    /// subsequent calculation. THe result is placed into the 'from' sub grid.
    /// </summary>
    private void MergeIntermediaryResults(ClientHeightAndTimeLeafSubGrid heightGrid1, ClientHeightAndTimeLeafSubGrid intermediaryHeightGrid)
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
    /// Processes each sub grid in turn into the resulting profile.
    /// </summary>
    public async Task ProcessSubGroup(SubGridCellAddress address, bool prodDataAtAddress, SubGridTreeBitmapSubGridBits cellOverrideMask)
    { 
      bool okToProceed = false;

      // Execute a client grid request for each requester and create an array of the results
      var clientGrids = Requestors.Select(async x =>
      {
        x.CellOverrideMask = cellOverrideMask;

        // Reach into the sub grid request layer and retrieve an appropriate sub grid
        var requestSubGridInternalResult = await x.RequestSubGridInternal(address, prodDataAtAddress, true);
        if (requestSubGridInternalResult.requestResult != ServerRequestResult.NoError)
          Log.LogError($"Request for sub grid {address} request failed with code {requestSubGridInternalResult.requestResult}");

        return requestSubGridInternalResult.clientGrid;
      }).ToArray();

      await Task.WhenAll(clientGrids);

      // If an intermediary result was requested then merge the 'from' and intermediary sub grids now
      if (IntermediaryFilterRequired)
      {
        MergeIntermediaryResults(await clientGrids[0] as ClientHeightAndTimeLeafSubGrid, await clientGrids[1] as ClientHeightAndTimeLeafSubGrid);
        //... and chop out the intermediary grid
        clientGrids = new[] { clientGrids[0], clientGrids[2] };
      }

      // Assign the results of the sub grid requests according to the ordering of the filter in the overall
      // volume type context of the request
      ClientHeightAndTimeLeafSubGrid heightsGrid1 = null;
      if (VolumeType == VolumeComputationType.BetweenFilterAndDesign || VolumeType == VolumeComputationType.Between2Filters)
        heightsGrid1 = await clientGrids[0] as ClientHeightAndTimeLeafSubGrid;

      ClientHeightAndTimeLeafSubGrid heightsGrid2 = null;
      if (VolumeType == VolumeComputationType.BetweenDesignAndFilter)
        heightsGrid2 = await clientGrids[0] as ClientHeightAndTimeLeafSubGrid;
      else if (VolumeType == VolumeComputationType.Between2Filters)
        heightsGrid2 = await clientGrids[1] as ClientHeightAndTimeLeafSubGrid;

      IClientHeightLeafSubGrid designHeights = null;

      if (VolumeType == VolumeComputationType.BetweenFilterAndDesign || VolumeType == VolumeComputationType.BetweenDesignAndFilter)
      {
        if (svDesignWrapper?.Design != null)
        {
          var getDesignHeightsResult = await svDesignWrapper.Design.GetDesignHeights(SiteModel.ID, svDesignWrapper.Offset, address, SiteModel.CellSize);

          if (getDesignHeightsResult.errorCode != DesignProfilerRequestResult.OK || getDesignHeightsResult.designHeights == null)
          {
            if (getDesignHeightsResult.errorCode == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
              Log.LogInformation("Call to RequestDesignElevationPatch failed due to no elevations in requested patch.");
            else
              Log.LogError($"Call to RequestDesignElevationPatch failed due to no TDesignProfilerRequestResult return code {getDesignHeightsResult.errorCode}.");
          }
          else
          {
            designHeights = getDesignHeightsResult.designHeights;
            okToProceed = true;
          }
        }
        else
          Log.LogError("Missing design reference. Call to request Summary Volumes Profile using design failed due to no reference design");
      }
      else
        okToProceed = true;

      if (okToProceed)
      {
        for (int I = 0; I < cellCounter; I++)
        {
          int cellX = profileCellList[I].OTGCellX & SubGridTreeConsts.SubGridLocalKeyMask;
          int cellY = profileCellList[I].OTGCellY & SubGridTreeConsts.SubGridLocalKeyMask;
          if (heightsGrid1 != null)
            profileCellList[I].LastCellPassElevation1 = heightsGrid1.Cells[cellX, cellY];
          if (heightsGrid2 != null)
            profileCellList[I].LastCellPassElevation2 = heightsGrid2.Cells[cellX, cellY];
          profileCellList[I].DesignElev = designHeights?.Cells[cellX, cellY] ?? CellPassConsts.NullHeight;
        }
      }
    }

    ///  <summary>
    ///  Builds a fully analyzed vector of profiled cells from the list of cell passed to it
    ///  </summary>
    /// <param name="profileCells"></param>
    /// <param name="cellPassIterator"></param>
    /// <returns></returns>
    public override async Task<bool> Analyze(List<SummaryVolumeProfileCell> profileCells, ISubGridSegmentCellPassIterator cellPassIterator)
    {

      Log.LogDebug($"Analyze Summary Volume ProfileCells. Processing {profileCells.Count}");

      var CurrentSubgridOrigin = new SubGridCellAddress(int.MaxValue, int.MaxValue);
      ISubGrid SubGrid = null;
      IServerLeafSubGrid _SubGridAsLeaf = null;
      profileCell = null;

      // Construct the set of requestors to query elevation sub grids needed for the summary volume calculations.
      var utilities = DIContext.Obtain<IRequestorUtilities>();
      Requestors = utilities.ConstructRequestors(null, SiteModel, Overrides, LiftParams,
        utilities.ConstructRequestorIntermediaries(SiteModel, ConstructFilters(), true, GridDataType.HeightAndTime),
        AreaControlSet.CreateAreaControlSet(), PDExistenceMap);

      var cellOverrideMask = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

      for (int I = 0; I < profileCells.Count; I++)
      {
        profileCell = profileCells[I];

        // get sub grid origin for cell address
        var thisSubgridOrigin = new SubGridCellAddress(profileCell.OTGCellX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
                                                       profileCell.OTGCellY >> SubGridTreeConsts.SubGridIndexBitsPerLevel);

        if (!CurrentSubgridOrigin.Equals(thisSubgridOrigin)) // if we have a new sub grid to fetch 
        {
          // if we have an existing sub grid and a change in sub grid detected process the current sub grid profile cell list
          if (SubGrid != null)
          {
            await ProcessSubGroup(new SubGridCellAddress(CurrentSubgridOrigin.X << SubGridTreeConsts.SubGridIndexBitsPerLevel, CurrentSubgridOrigin.Y << SubGridTreeConsts.SubGridIndexBitsPerLevel),
                            PDExistenceMap[CurrentSubgridOrigin.X, CurrentSubgridOrigin.Y], cellOverrideMask);
            cellOverrideMask.Clear();
          }

          SubGrid = null;
          cellCounter = 0;

          // Does the sub grid tree contain this node in it's existence map? if so get sub grid
          if (PDExistenceMap[thisSubgridOrigin.X, thisSubgridOrigin.Y])
            SubGrid = SubGridTrees.Server.Utilities.SubGridUtilities.LocateSubGridContaining
              (SiteModel.PrimaryStorageProxy, SiteModel.Grid, profileCell.OTGCellX, profileCell.OTGCellY, SiteModel.Grid.NumLevels, false, false);

          _SubGridAsLeaf = SubGrid as ServerSubGridTreeLeaf;
          if (_SubGridAsLeaf == null)
            continue;

          CurrentSubgridOrigin = thisSubgridOrigin; // all good to proceed with this sub grid
        }

        profileCellList[cellCounter++] = profileCell; // add cell to list to process for this sub grid
        cellOverrideMask.SetBit(profileCell.OTGCellX & SubGridTreeConsts.SubGridLocalKeyMask, profileCell.OTGCellY & SubGridTreeConsts.SubGridLocalKeyMask);
      }

      if (cellCounter > 0 && SubGrid != null) // Make sure we process last list
      {
        await ProcessSubGroup(new SubGridCellAddress(CurrentSubgridOrigin.X << SubGridTreeConsts.SubGridIndexBitsPerLevel, CurrentSubgridOrigin.Y << SubGridTreeConsts.SubGridIndexBitsPerLevel),
                        PDExistenceMap[CurrentSubgridOrigin.X, CurrentSubgridOrigin.Y], cellOverrideMask);
      }

      return true;
    }
  }
}

