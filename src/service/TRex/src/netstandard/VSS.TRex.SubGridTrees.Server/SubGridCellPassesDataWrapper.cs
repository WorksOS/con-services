using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{
  public class SubGridCellPassesDataWrapper : ISubGridCellPassesDataWrapper
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridCellPassesDataWrapper>();

    private IServerLeafSubGrid _owner;
    public IServerLeafSubGrid Owner { get => _owner; set => _owner = value; }

    private ISubGridCellPassesDataSegments _passesData = new SubGridCellPassesDataSegments();
    public ISubGridCellPassesDataSegments PassesData { get => _passesData; set => _passesData = value; }

    private static readonly int _subGridSegmentPassCountLimit = DIContext.Obtain<IConfigurationStore>().GetValueInt("VLPDSUBGRID_SEGMENTPASSCOUNTLIMIT", Consts.VLPDSUBGRID_SEGMENTPASSCOUNTLIMIT);

    public void Clear()
    {
      _passesData.Clear();
    }

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
      var passesCount = _passesData.Count;

      if (_owner.Directory.SegmentDirectory.Count == 0)
      {
        if (passesCount != 0)
        {
          Log.LogCritical($"Passes segment list for {_owner.Moniker()} is non-empty when the segment info list is empty in SelectSegment");
          return null;
        }

        _owner.CreateDefaultSegment();
        _owner.AllocateSegment(_owner.Directory.SegmentDirectory.First());
        return _owner.Directory.SegmentDirectory.First().Segment;
      }

      if (passesCount == 0)
      {
        Log.LogCritical($"Passes data array empty for {_owner.Moniker()} in SelectSegment");
        return null;
      }

      int Index = 0;
      int DirectionIncrement = 0;

      while (true)
      {
        var testPass = _passesData[Index];

        if (testPass.SegmentMatches(time))
        {
          return testPass;
        }

        if (DirectionIncrement == 0)
        {
          DirectionIncrement = time < testPass.SegmentInfo.StartTime ? -1 : 1;

          Index += DirectionIncrement;
        }

        if (!(Index >= 0 || Index <= passesCount - 1))
        {
          return null;
        }

        if (DirectionIncrement == 1)
        {
          if (time < testPass.SegmentInfo.StartTime)
          {
            return null;
          }
        }
        else
        {
          if (time > testPass.SegmentInfo.EndTime)
          {
            return null;
          }
        }
      }
    }

    /// <summary>
    /// CleaveSegment does the donkey work of taking a segment and splitting it into
    /// two segments that each contain half the cell passes of the segment passed in
    /// If the subGridSegmentPassCountLimit is zero then the default segment pass count
    /// limit specified in the configuration will be used.
    /// </summary>
    /// <param name="CleavingSegment"></param>
    /// <param name="PersistedClovenSegments"></param>
    /// <param name="subGridSegmentPassCountLimit"></param>
    /// <returns></returns>
    public bool CleaveSegment(ISubGridCellPassesDataSegment CleavingSegment,
      List<ISubGridSpatialAffinityKey> PersistedClovenSegments,
      int subGridSegmentPassCountLimit = 0)
    {
      if (!CleavingSegment.HasAllPasses)
      {
        Log.LogError($"Cannot cleave a sub grid ({_owner.Moniker()}) without its cell passes");
        return false;
      }

      if (subGridSegmentPassCountLimit == 0)
      {
        subGridSegmentPassCountLimit = _subGridSegmentPassCountLimit;
      }

      // Count up the number of cell passes in total in the segment
      CleavingSegment.PassesData.CalculateTotalPasses(out int TotalPassCount, out int _, out int _);

#if DEBUG
      CleavingSegment.VerifyComputedAndRecordedSegmentTimeRangeBounds();
#endif

      if (TotalPassCount <= subGridSegmentPassCountLimit)
      {
        Log.LogInformation("There is no need to cleave this segment as it's number of passes does not breach the limit");
        return false; // There is no need to cleave this segment
      }

      int NumRequiredClovenSegments = (TotalPassCount - 1) / subGridSegmentPassCountLimit + 1;

      // Determine the actual time range of the passes within the segment
      CleavingSegment.PassesData.CalculateTimeRange(out var CoveredTimeRangeStart, out var CoveredTimeRangeEnd);

      if (CoveredTimeRangeStart >= CoveredTimeRangeEnd) // There's nothing to do
      {
        Log.LogWarning($"Segment cleaving resolved no time range to assign cloven cell passes to. No cleaving performed. TotalPassCount = {TotalPassCount}, CoveredTimeRangeStart = {CoveredTimeRangeStart}, CoveredTimeRangeEnd {CoveredTimeRangeEnd}, NumRequiredClovenSegments = {NumRequiredClovenSegments}");
        return false;
      }

      // Preserve the allotted time range of the segment being cleaved
      DateTime OldEndTime = CleavingSegment.SegmentInfo.EndTime;

      // Record the segment that was cleaved in the list of those that need to be removed
      // when the sub grid is next persisted (but only if it has previously been persisted
      // to disk). If not, it is only in memory and can represent one part of the set
      // of cloven segments waiting for persistence.
      if (CleavingSegment.SegmentInfo.ExistsInPersistentStore)
        PersistedClovenSegments.Add(CleavingSegment.SegmentInfo.AffinityKey(_owner.Parent.Owner.ID));

      // Look for the time that splits the segment. Stop when we are within 100 cell passes
      // of the exact time.

      DateTime TestTimeRangeStart = CoveredTimeRangeStart;
      DateTime TestTimeRangeEnd = CoveredTimeRangeEnd;
      int PassesInFirstTimeRange;
      DateTime TestTime;
        
      int desiredCallPassCount = TotalPassCount / NumRequiredClovenSegments;
      do
      {
        TestTime = TestTimeRangeStart + new TimeSpan((TestTimeRangeEnd.Ticks - TestTimeRangeStart.Ticks) / 2);

        CleavingSegment.PassesData.CalculatePassesBeforeTime(TestTime, out PassesInFirstTimeRange, out int _);

        if (PassesInFirstTimeRange < desiredCallPassCount)
          TestTimeRangeStart = TestTime;
        else
          TestTimeRangeEnd = TestTime;
      } while (Math.Abs(PassesInFirstTimeRange - desiredCallPassCount) > 100);

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
      var NewSegmentInfo = new SubGridCellPassesDataSegmentInfo
      {
        StartTime = TestTime,
        EndTime = OldEndTime,
        Segment = NewSegment
      };

      NewSegment.SegmentInfo = NewSegmentInfo;

      // Add the newly created segment info to the segment info list
      _owner.Directory.SegmentDirectory.Insert(
        _owner.Directory.SegmentDirectory.IndexOf(CleavingSegment.SegmentInfo) + 1, NewSegmentInfo);

      // Add the new created segment into the segment list for the sub grid
      PassesData.Add(NewSegment);

      // Tidy up, marking both segments as dirty, and not existing in the persistent data store
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
                  Format('Failed to touch newly created segment in segment cleaving for sub grid %s [%s]', [
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
          $"Info: After cleave first segment {_owner.Directory.SegmentDirectory.IndexOf(CleavingSegment.SegmentInfo)} ({CleavingSegment.SegmentInfo.StartTime}-{CleavingSegment.SegmentInfo.EndTime}) of subgrid {_owner.Moniker()} failed to reduce cell pass count below maximums");

      NewSegment.VerifyComputedAndRecordedSegmentTimeRangeBounds();
      if (NewSegment.RequiresCleaving(out _, out _))
        Log.LogDebug(
          $"Info: New cloven segment {_owner.Directory.SegmentDirectory.IndexOf(CleavingSegment.SegmentInfo)} ({CleavingSegment.SegmentInfo.StartTime}-{OldEndTime}) (resulting from cleave) of subgrid {_owner.Moniker()} failed to reduce cell pass count below maximums");
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
