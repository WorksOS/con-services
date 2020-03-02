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

    private static readonly int SubGridSegmentPassCountLimit = DIContext.Obtain<IConfigurationStore>().GetValueInt("VLPDSUBGRID_SEGMENTPASSCOUNTLIMIT", Consts.VLPDSUBGRID_SEGMENTPASSCOUNTLIMIT);

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

      var index = 0;
      var directionIncrement = 0;

      while (true)
      {
        var testPass = _passesData[index];

        if (testPass.SegmentMatches(time))
        {
          return testPass;
        }

        if (directionIncrement == 0)
        {
          directionIncrement = time < testPass.SegmentInfo.StartTime ? -1 : 1;

          index += directionIncrement;
        }

        if (!(index >= 0 || index <= passesCount - 1))
        {
          return null;
        }

        if (directionIncrement == 1)
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
    /// <param name="cleavingSegment"></param>
    /// <param name="persistedClovenSegments"></param>
    /// <param name="subGridSegmentPassCountLimit"></param>
    /// <returns></returns>
    public bool CleaveSegment(ISubGridCellPassesDataSegment cleavingSegment,
      List<ISubGridSpatialAffinityKey> persistedClovenSegments,
      int subGridSegmentPassCountLimit = 0)
    {
      const int cleaveSegmentPassCountDeadBand = 100;
     
      if (!cleavingSegment.HasAllPasses)
      {
        Log.LogError($"Cannot cleave a sub grid ({_owner.Moniker()}) without its cell passes");
        return false;
      }

      if (subGridSegmentPassCountLimit == 0)
      {
        subGridSegmentPassCountLimit = SubGridSegmentPassCountLimit;
      }

      // Count up the number of cell passes in total in the segment
      cleavingSegment.PassesData.CalculateTotalPasses(out var totalPassCount, out var _, out var _);

#if DEBUG
      cleavingSegment.VerifyComputedAndRecordedSegmentTimeRangeBounds();
#endif

      if (totalPassCount <= subGridSegmentPassCountLimit)
      {
        Log.LogInformation("There is no need to cleave this segment as it's number of passes does not breach the limit");
        return false; // There is no need to cleave this segment
      }

      // Don't resize to exactly the limit as this means cell passes added due to our of order processing will cause
      // more extensive cleaving. Set it to two thirds of the limit that triggers cleaving
      var subGridSegmentPassCountLimitWithHeadroom = (2 * subGridSegmentPassCountLimit) / 3;

      var numRequiredClovenSegments = (totalPassCount - 1) / subGridSegmentPassCountLimitWithHeadroom + 1;

      // Determine the actual time range of the passes within the segment
      cleavingSegment.PassesData.CalculateTimeRange(out var coveredTimeRangeStart, out var coveredTimeRangeEnd);

      if (coveredTimeRangeStart >= coveredTimeRangeEnd) // There's nothing to do
      {
        Log.LogWarning($"Segment cleaving resolved no time range to assign cloven cell passes to. No cleaving performed. TotalPassCount = {totalPassCount}, CoveredTimeRangeStart = {coveredTimeRangeStart}, CoveredTimeRangeEnd {coveredTimeRangeEnd}, NumRequiredClovenSegments = {numRequiredClovenSegments}");
        return false;
      }

      // Preserve the allotted time range of the segment being cleaved
      var oldEndTime = cleavingSegment.SegmentInfo.EndTime;

      // Record the segment that was cleaved in the list of those that need to be removed
      // when the sub grid is next persisted (but only if it has previously been persisted
      // to disk). If not, it is only in memory and can represent one part of the set
      // of cloven segments waiting for persistence.
      if (cleavingSegment.SegmentInfo.ExistsInPersistentStore)
        persistedClovenSegments.Add(cleavingSegment.SegmentInfo.AffinityKey(_owner.Parent.Owner.ID));

      // Look for the time that splits the segment. Stop when we are within CleaveSegmentPassCountDeadBand cell passes
      // of the exact time.

      var testTimeRangeStart = coveredTimeRangeStart;
      var testTimeRangeEnd = coveredTimeRangeEnd;
      int passesInFirstTimeRange;
      DateTime testTime;
        
      var desiredCallPassCount = totalPassCount / numRequiredClovenSegments;
      do
      {
        testTime = testTimeRangeStart + new TimeSpan((testTimeRangeEnd.Ticks - testTimeRangeStart.Ticks) / 2);

        cleavingSegment.PassesData.CalculatePassesBeforeTime(testTime, out passesInFirstTimeRange, out var _);

        if (passesInFirstTimeRange < desiredCallPassCount)
          testTimeRangeStart = testTime;
        else
          testTimeRangeEnd = testTime;
      } while (Math.Abs(passesInFirstTimeRange - desiredCallPassCount) > cleaveSegmentPassCountDeadBand);

      // Create the new segment that will contain the overflow from the given segment
      // and copy the cell passes after the test time from the segment being cleaved
      // into the new segment
      //ISubGridCellSegmentPassesDataWrapper NewSegment = SubGridCellSegmentPassesDataWrapperFactory.Instance().NewWrapper(true, false);
      var newSegment = new SubGridCellPassesDataSegment
      {
        Owner = cleavingSegment.Owner
      };

      newSegment.AllocateFullPassStacks();
      newSegment.PassesData.AdoptCellPassesFrom(cleavingSegment.PassesData, testTime);

      // Modify the time range of segment to match it's new time span and set up
      // the segment info for the newly created segment
      cleavingSegment.SegmentInfo.EndTime = testTime;

      // Create a new segment info entry for the new segment and add it into the segment directory
      // so it fills the time space just created by modifying the end time of the
      // segment being cleaved
      var newSegmentInfo = new SubGridCellPassesDataSegmentInfo
      {
        StartTime = testTime,
        EndTime = oldEndTime,
        Segment = newSegment
      };

      newSegment.SegmentInfo = newSegmentInfo;

      // Add the newly created segment info to the segment info list
      _owner.Directory.SegmentDirectory.Insert(
        _owner.Directory.SegmentDirectory.IndexOf(cleavingSegment.SegmentInfo) + 1, newSegmentInfo);

      // Add the new created segment into the segment list for the sub grid
      PassesData.Add(newSegment);

      // Tidy up, marking both segments as dirty, and not existing in the persistent data store
      cleavingSegment.Dirty = true;
      cleavingSegment.SegmentInfo.ExistsInPersistentStore = false;

      newSegment.Dirty = true;
      newSegment.SegmentInfo.ExistsInPersistentStore = false;

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
      cleavingSegment.VerifyComputedAndRecordedSegmentTimeRangeBounds();
      if (cleavingSegment.RequiresCleaving(out var totalPasses, out var maxPassCount))
        Log.LogDebug(
          $"Info: After cleave first segment {_owner.Directory.SegmentDirectory.IndexOf(cleavingSegment.SegmentInfo)} ({cleavingSegment.SegmentInfo.StartTime}-{cleavingSegment.SegmentInfo.EndTime}) of subgrid {_owner.Moniker()} failed to reduce cell pass count below maximums. Total passes = {totalPasses}, max pass count = {maxPassCount}");

      /* Note: newSegment will contain the set of cell passes that were moved out of cleavingSegment to ensure the size of that segment was below the limit.
       newSegment itself may have additional necessary cleavings before being under the cap.
       newSegment.VerifyComputedAndRecordedSegmentTimeRangeBounds();
       if (newSegment.RequiresCleaving(out totalPasses, out maxPassCount))
         Log.LogDebug(
           $"Info: New cloven segment {_owner.Directory.SegmentDirectory.IndexOf(cleavingSegment.SegmentInfo)} ({cleavingSegment.SegmentInfo.StartTime}-{oldEndTime}) (resulting from cleave) of subgrid {_owner.Moniker()} failed to reduce cell pass count below maximums. Total passes = {totalPasses}, max pass count = {maxPassCount}");
      */
#endif

      return true;
    }

    public bool MergeSegments(ISubGridCellPassesDataSegment mergeToSegment,
      ISubGridCellPassesDataSegment mergeFromSegment)
    {
      return false;
    }

    public void RemoveSegment(ISubGridCellPassesDataSegment segment)
    {
    }
  }
}
