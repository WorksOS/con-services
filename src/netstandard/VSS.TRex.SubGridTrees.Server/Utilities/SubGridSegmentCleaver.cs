using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Iterators;

namespace VSS.TRex.SubGridTrees.Server.Utilities
{
  /// <summary>
  /// Provides segment cleaving semantics against the set of segments contained within a subgrid
  /// </summary>
  public class SubGridSegmentCleaver
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridSegmentCleaver>();

    private int _subGridSegmentPassCountLimit = Consts.kVlpdSubGridSegmentPassCountLimitDefault;

    private int _subGridMaxSegmentCellPassesLimit = Consts.kVlpdSubGridMaxSegmentCellPassesLimitDefault;

    private bool _segmentCleavingOperationsToLog = Consts.kSegmentCleavingOperationsToLogDefault;

    private void ReadEnvironmentVariables()
    {
      var config = DIContext.Obtain<IConfigurationStore>();
      var configResult = config.GetValueInt("VLPDSUBGRID_SEGMENTPASSCOUNTLIMIT");
      if (configResult > -1)
      {
        _subGridSegmentPassCountLimit = configResult;
      }
      configResult = config.GetValueInt("VLPDSUBGRID_MAXSEGMENTCELLPASSESLIMIT");
      if (configResult > -1)
      {
        _subGridMaxSegmentCellPassesLimit = configResult;
      }
      var configResultBool = config.GetValueBool("SEGMENTCLEAVINGOOPERATIONSTOLOG");
      if (configResultBool != null)
      {
        _segmentCleavingOperationsToLog = configResultBool.Value;
      }
    }

    public SubGridSegmentCleaver()
    {
      ReadEnvironmentVariables();
    }

    // PersistedClovenSegments contains a list of all the segments that exists in the
    // persistent data store that have been cloven since the last time this leaf
    // was persisted to the data store. This is essentially a list of obsolete
    // segments whose presence in the persistent data store need to be removed
    // when the subgrid is next persisted
    public List<ISubGridSpatialAffinityKey> PersistedClovenSegments { get; } = new List<ISubGridSpatialAffinityKey>(10);

    /// <summary>
    /// Cleaves all dirty segments requiring cleaving within the given subgrid
    /// </summary>
    /// <param name="storageProxy"></param>
    /// <param name="subGrid"></param>
    public void PerformSegmentCleaving(IStorageProxy storageProxy, IServerLeafSubGrid subGrid)
    {
      SubGridSegmentIterator Iterator = new SubGridSegmentIterator(subGrid, storageProxy)
      {
        IterationDirection = IterationDirection.Forwards,
        ReturnDirtyOnly = true,
        RetrieveAllPasses = true
      };

      SubGridCellAddress Origin = new SubGridCellAddress(subGrid.OriginX, subGrid.OriginY);

      if (!Iterator.MoveToFirstSubGridSegment())
        return;

      do
      {
        ISubGridCellPassesDataSegment Segment = Iterator.CurrentSubGridSegment;

        DateTime CleavedTimeRangeStart = Segment.SegmentInfo.StartTime;
        DateTime CleavedTimeRangeEnd = Segment.SegmentInfo.EndTime;

        if (Segment.RequiresCleaving(out uint TotalPassCount, out uint MaximumPassCount))
        {
          if (subGrid.Cells.CleaveSegment(Segment, PersistedClovenSegments))
          {
              Iterator.SegmentListExtended();

              if (_segmentCleavingOperationsToLog)
                Log.LogInformation($"Info: Performed cleave on segment ({CleavedTimeRangeStart}-{CleavedTimeRangeEnd}) of subgrid {ServerSubGridTree.GetLeafSubGridFullFileName(Origin)}");
          }
          else
          {
            // The segment cleave failed. Currently the only cause of this is a
            // database modification lock acquisition failure. While this is not
            // a serious problem (as the subgrid will be cleaved at some point in
            // the future when it is modified again via tag file processing etc)
            // it will be noted in the log.

            if (_segmentCleavingOperationsToLog)
              Log.LogInformation($"Info: Cleave on segment ({CleavedTimeRangeStart}-{CleavedTimeRangeEnd}) of subgrid {ServerSubGridTree.GetLeafSubGridFullFileName(Origin)} failed");
          }
        }

        if (_segmentCleavingOperationsToLog)
        {
          if (Segment.RequiresCleaving(out TotalPassCount, out MaximumPassCount))
            Log.LogInformation(
              $"Info: Cleave on segment ({CleavedTimeRangeStart}-{CleavedTimeRangeEnd}) of subgrid {subGrid.Moniker()} failed to reduce cell pass count below maximums (max passes = {TotalPassCount}/{_subGridSegmentPassCountLimit}, per cell = {MaximumPassCount}/{_subGridMaxSegmentCellPassesLimit})");
        }
      } while (Iterator.MoveToNextSubGridSegment());
    }
  }
}
