using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Types;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
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
    public VolumeComputationType VolumeType { get; set; } = VolumeComputationType.None;

    private SummaryVolumesCellProfileAnalyzer()
    {}

    /// <summary>
    /// Constructs a profile lift builder that analyzes cells in a cell profile vector
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="pDExistenceMap"></param>
    /// <param name="filterSet"></param>
    /// <param name="cellPassFilter_ElevationRangeDesign"></param>
    /// <param name="cellLiftBuilder"></param>
    public SummaryVolumesCellProfileAnalyzer(ISiteModel siteModel,
      ISubGridTreeBitMask pDExistenceMap,
      IFilterSet filterSet,
      IDesign cellPassFilter_ElevationRangeDesign,
      ICellLiftBuilder cellLiftBuilder) : base(siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesign)
    {
    }


    public void ProcessSubGroup()
    {


      // This this subgrid get relevant data based upon VolumeType requested

      int I,J,K;
      int YCell;
      float H1, H2;
      float StationAtNextCellBorder;
      bool OKToAdd;

      if (VolumeType == VolumeComputationType.BetweenFilterAndDesign || VolumeType == VolumeComputationType.Between2Filters)
      {
        var acs = new AreaControlSet();
        // to do get subgrid
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
      //{$IFDEF DEBUG}
      //SIGLogMessage.PublishNoODS(Self, Format('BuildLiftProfileFromInitialLayer: Processing %d cells', [FProfileCells.Count]), slmcDebug);
      //{$ENDIF}

      SubGridCellAddress CurrentSubgridOrigin = new SubGridCellAddress(int.MaxValue, int.MaxValue);
      ISubGrid SubGrid = null;
      IServerLeafSubGrid _SubGridAsLeaf = null;
      profileCell = null;
      //      FilterDesignElevations = null;
      bool IgnoreSubgrid = false;

      for (int I = 0; I < profileCells.Count; I++)
      {

        profileCell = profileCells[I];

        // get subgrid setup iterator and set cell address
        // get subgrid origin for cell address
        SubGridCellAddress thisSubgridOrigin = new SubGridCellAddress(profileCell.OTGCellX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
          profileCell.OTGCellY >> SubGridTreeConsts.SubGridIndexBitsPerLevel);

        if (!CurrentSubgridOrigin.Equals(thisSubgridOrigin)) // if we have a new subgrid to fetch
        {
          IgnoreSubgrid = false;
          CurrentSubgridOrigin = thisSubgridOrigin;
          SubGrid = null;

          // Does the subgrid tree contain this node in it's existence map?
          if (PDExistenceMap[CurrentSubgridOrigin.X, CurrentSubgridOrigin.Y])
            SubGrid = SubGridTrees.Server.Utilities.SubGridUtilities.LocateSubGridContaining
              (StorageProxy, SiteModel.Grid, profileCell.OTGCellX, profileCell.OTGCellY, SiteModel.Grid.NumLevels, false, false);

          _SubGridAsLeaf = SubGrid as ServerSubGridTreeLeaf;
          if (_SubGridAsLeaf == null)
            continue;
        }

        if (SubGrid != null && !IgnoreSubgrid)
        {
          ProcessSubGroup();
        }
      }

      return true;
    }
  }
}
