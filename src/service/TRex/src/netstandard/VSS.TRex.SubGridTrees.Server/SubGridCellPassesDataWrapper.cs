using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{
  public class SubGridCellPassesDataWrapper : ISubGridCellPassesDataWrapper
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridCellPassesDataWrapper>();

    public IServerLeafSubGrid Owner { get; set; }

    public void Clear()
    {
      PassesData.Clear();
    }

    public ISubGridCellPassesDataSegments PassesData { get; set; } = new SubGridCellPassesDataSegments();

    private readonly int _subGridSegmentPassCountLimit = DIContext.Obtain<IConfigurationStore>().GetValueInt("VLPDSUBGRID_SEGMENTPASSCOUNTLIMIT", Consts.kVlpdSubGridSegmentPassCountLimitDefault);

    public SubGridCellPassesDataWrapper()
    {
      Clear();
    }

    public void Initialise()
    {
      Clear();
    }

    public ISubGridCellPassesDataSegment SelectSegment(DateTime time)
    {
      if (Owner.Directory.SegmentDirectory.Count == 0)
      {
        if (PassesData.Count != 0)
        {
          Log.LogCritical($"Passes segment list for {Owner.Moniker()} is non-empty when the segment info list is empty in SelectSegment");
          return null;
        }

        Owner.CreateDefaultSegment();
        Owner.AllocateSegment(Owner.Directory.SegmentDirectory.First());
        return Owner.Directory.SegmentDirectory.First().Segment;
      }

      if (PassesData.Count == 0)
      {
        Log.LogCritical($"Passes data array empty for {Owner.Moniker()} in SelectSegment");
        return null;
      }

      int Index = 0;
      int DirectionIncrement = 0;

      while (true)
      {
        if (PassesData[Index].SegmentMatches(time))
        {
          return PassesData[Index];
        }

        if (DirectionIncrement == 0)
        {
          DirectionIncrement = time < PassesData[Index].SegmentInfo.StartTime ? -1 : 1;

          Index += DirectionIncrement;
        }

        if (!(Index >= 0 || Index <= PassesData.Count - 1))
        {
          return null;
        }

        if (DirectionIncrement == 1)
        {
          if (time < PassesData[Index].SegmentInfo.StartTime)
          {
            return null;
          }
        }
        else
        {
          if (time > PassesData[Index].SegmentInfo.EndTime)
          {
            return null;
          }
        }
      }
    }

    // CleaveSegment does the donkey work of taking a segment and splitting it into
    // two segments that each contain half the cell passes of the segment passed in
    public bool CleaveSegment(ISubGridCellPassesDataSegment CleavingSegment,
      List<ISubGridSpatialAffinityKey> PersistedClovenSegments)
    {
      if (!CleavingSegment.HasAllPasses)
      {
        Log.LogError($"Cannot cleave a subgrid ({Owner.Moniker()}) without its cell passes");
        return false;
      }

      // Count up the number of cell passes in total in the segment
      CleavingSegment.PassesData.CalculateTotalPasses(out uint TotalPassCount, out uint _ /*MaximumPassCount*/);

#if DEBUG
      CleavingSegment.VerifyComputedAndRecordedSegmentTimeRangeBounds();
#endif

      if (TotalPassCount < _subGridSegmentPassCountLimit)
      {
        return false; // There is no need to cleave this segment
      }

      int NumRequiredClovenSegments = ((int) TotalPassCount - 1) / _subGridSegmentPassCountLimit + 1;

      // Determine the actual time range of the passes within the segment
      CleavingSegment.PassesData.CalculateTimeRange(out DateTime CoveredTimeRangeStart,
        out DateTime CoveredTimeRangeEnd);

      if (CoveredTimeRangeStart >= CoveredTimeRangeEnd) // There's nothing to do
        return false;

      // Preserve the allotted time range of the segment being cleaved
      DateTime OldEndTime = CleavingSegment.SegmentInfo.EndTime;

      // Record the segment that was cleaved in the list of those that need to be removed
      // when the subgrid is next persisted (but only if it has previously been persisted
      // to disk). If not, it is only in memory and can represent one part of the set
      // of cloven segments waiting for persistence.
      if (CleavingSegment.SegmentInfo.ExistsInPersistentStore)
        PersistedClovenSegments.Add(CleavingSegment.SegmentInfo.AffinityKey(Owner.Parent.Owner.ID));

      // Look for the time that splits the segment. Stop when we are within 100 cell passes
      // of the exact time.

      DateTime TestTimeRangeStart = CoveredTimeRangeStart;
      DateTime TestTimeRangeEnd = CoveredTimeRangeEnd;
      uint PassesInFirstTimeRange;
      DateTime TestTime;

      do
      {
        TestTime = TestTimeRangeStart + new TimeSpan((TestTimeRangeEnd.Ticks - TestTimeRangeStart.Ticks) / 2);

        CleavingSegment.PassesData.CalculatePassesBeforeTime(TestTime, out PassesInFirstTimeRange, out uint _);

        if (PassesInFirstTimeRange < TotalPassCount / NumRequiredClovenSegments)
          TestTimeRangeStart = TestTime;
        else
          TestTimeRangeEnd = TestTime;
      } while (Math.Abs(PassesInFirstTimeRange - TotalPassCount / NumRequiredClovenSegments) > 100);

      // Create the new segment that will contain the overflow from the given segment
      // and copy the cell passes after the test time from the segment being cleaved
      // into the new segment
      //ISubGridCellSegmentPassesDataWrapper NewSegment = SubGridCellSegmentPassesDataWrapperFactory.Instance().NewWrapper(true, false);
      ISubGridCellPassesDataSegment NewSegment = new SubGridCellPassesDataSegment
      {
        Owner = CleavingSegment.Owner
      };

      NewSegment.AllocateFullPassStacks();
      NewSegment.PassesData.AdoptCellPassesFrom(CleavingSegment.PassesData, TestTime);

      // Modify the time range of Segment to match it's new time span and set up
      // the segment info for the newly created segment
      CleavingSegment.SegmentInfo.EndTime = TestTime;

      // Create a new segment info entry for the new segment and add it into the segment directory
      // so it fills the time space just created by modifying the end time of the
      // segment being cleaved
      SubGridCellPassesDataSegmentInfo NewSegmentInfo = new SubGridCellPassesDataSegmentInfo
      {
        StartTime = TestTime,
        EndTime = OldEndTime,
        Segment = NewSegment
      };

      NewSegment.SegmentInfo = NewSegmentInfo;

      // Add the newly created segment info to the segment info list
      Owner.Directory.SegmentDirectory.Insert(
        Owner.Directory.SegmentDirectory.IndexOf(CleavingSegment.SegmentInfo) + 1, NewSegmentInfo);

      // Add the new created segment into the segment list for the subgrid
      PassesData.Add(NewSegment);

      // Tidy up, marking both segments as dirty, and not existing in the persistent data store!
      CleavingSegment.Dirty = true;
      CleavingSegment.SegmentInfo.ExistsInPersistentStore = false;

      NewSegment.Dirty = true;
      NewSegment.SegmentInfo.ExistsInPersistentStore = false;

      /*
       TODO: In TRex, this non-static information is only maintained in the mutable data grid, segment caching is not well defined for it yet
      if (Owner.PresentInCache)
      {
          // Include the new segment in the cache segment tracking
          if (!DataStoreInstance.GridDataCache.SubGridSegmentTouched(NewSegment))
          {
               SIGLogMessage.PublishNoODS(Self,
                  Format('Failed to touch newly created segment in segment cleaving for subgrid %s [%s]', [
                      CleavingSegment.Owner.Moniker, CleavingSegment.ToString]), slmcException);
              return true;
          }
      }
      */

#if DEBUG
      // Check everything looks kosher by comparing the time range of the cells
      // present in the cloven segments with the time range bounds in the segment
      // information for the segments

      // Determine the actual time range of the passes within the segment
      CleavingSegment.VerifyComputedAndRecordedSegmentTimeRangeBounds();
      if (CleavingSegment.RequiresCleaving(out _, out _))
        Log.LogDebug(
          $"Info: After cleave first segment {Owner.Directory.SegmentDirectory.IndexOf(CleavingSegment.SegmentInfo)} ({CleavingSegment.SegmentInfo.StartTime}-{CleavingSegment.SegmentInfo.EndTime}) of subgrid {Owner.Moniker()} failed to reduce cell pass count below maximums");

      NewSegment.VerifyComputedAndRecordedSegmentTimeRangeBounds();
      if (NewSegment.RequiresCleaving(out _, out _))
        Log.LogDebug(
          $"Info: New cloven segment {Owner.Directory.SegmentDirectory.IndexOf(CleavingSegment.SegmentInfo)} ({CleavingSegment.SegmentInfo.StartTime}-{OldEndTime}) (resulting from cleave) of subgrid %s failed to reduce cell pass count below maximums");
#endif

      return true;
    }

    public bool MergeSegments(ISubGridCellPassesDataSegment MergeToSegment,
      ISubGridCellPassesDataSegment MergeFromSegment)
    {
      return false;
    }

    public void RemoveSegment(ISubGridCellPassesDataSegment Segment)
    {
    }
  }
}
