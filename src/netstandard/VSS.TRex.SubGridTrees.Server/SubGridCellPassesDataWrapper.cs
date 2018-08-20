using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{
  public class SubGridCellPassesDataWrapper : ISubGridCellPassesDataWrapper
  {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        public IServerLeafSubGrid Owner { get; set; }   // FOwner : TICServerSubGridTreeLeaf;

        public void Clear()
        {
            PassesData.Clear();
        }

        public ISubGridCellPassesDataSegments PassesData { get; set; } = new SubGridCellPassesDataSegments();

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
                    // TODO: Add when logging available
                    //SIGLogMessage.PublishNoODS(Self, Format('Passes segment list for %s is non-empty when the segment info list is empty in TICSubGridCellPassesDataWrapper.SelectSegment', [FOwner.Moniker]), slmcAssert);
                    return null;
                }

                Owner.CreateDefaultSegment();
                Owner.AllocateSegment(Owner.Directory.SegmentDirectory.First());
                return Owner.Directory.SegmentDirectory.First().Segment;
            }

            if (PassesData.Count == 0)
            {
                // TODO: Add when logging available
                // SIGLogMessage.PublishNoODS(Self, Format('Passes data array empty for %s in TICSubGridCellPassesDataWrapper.SelectSegment', [FOwner.Moniker]), slmcAssert);
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
        public bool CleaveSegment(ISubGridCellPassesDataSegment CleavingSegment)
        {
            // TODO: Remove until locking semntics are reviewed
            //if (!Owner.Locked)
            //{
            //  Log.LogError(
           //         $"May not perform a segment cleave operation on a subgrid ({Owner.Moniker()}) that is not locked");
            //    return false;
            //}

            if (!CleavingSegment.HasAllPasses)
            {
                Log.LogError($"Cannot cleave a subgrid ({Owner.Moniker()}) without its cell passes");
                return false;
            }

            // Count up the number of cell passes in total in the segment
            CleavingSegment.PassesData.CalculateTotalPasses(out uint TotalPassCount, out uint MaximumPassCount);

            //todo {$IFDEF DEBUG}
            //CleavingSegment.VerifyComputedAndRecordedSegmentTimeRangeBounds;
            //{$ENDIF}

            if (TotalPassCount < TRexConfig.VLPD_SubGridSegmentPassCountLimit)
            {
                return false; // There is no need to cleave this segment
            }

            int NumRequiredClovenSegments = ((int) TotalPassCount - 1) / TRexConfig.VLPD_SubGridSegmentPassCountLimit + 1;

            // Determine the actual time range of the passes within the segment
            CleavingSegment.PassesData.CalculateTimeRange(out DateTime CoveredTimeRangeStart,
                out DateTime CoveredTimeRangeEnd);

            if (CoveredTimeRangeStart >= CoveredTimeRangeEnd) // There's nothing to do
                return false;

            // Preserve the allotted time range of the segment being cleaved
            DateTime OldEndTime = CleavingSegment.SegmentInfo.EndTime;

            // Look for the time that splits the segment. Stop when we are within 100 cell passes
            // of the exact time.

            DateTime TestTimeRangeStart = CoveredTimeRangeStart;
            DateTime TestTimeRangeEnd = CoveredTimeRangeEnd;
            uint PassesInFirstTimeRange;
            DateTime TestTime;

            do
            {
                TestTime = new DateTime((TestTimeRangeStart.Ticks + TestTimeRangeEnd.Ticks) / 2);

                CleavingSegment.PassesData.CalculatePassesBeforeTime(TestTime, out PassesInFirstTimeRange, out uint _);

                if (PassesInFirstTimeRange < (TotalPassCount / NumRequiredClovenSegments))
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

            // Record the segment that was cleaved in the list of those that need to be removed
            // when the subgrid is next persisted (but only if it has previously been persisted
            // to disk). If not, it is only in memory and can represent one part of the set
            // of cloven segments waiting for persistence.
            if (CleavingSegment.SegmentInfo.ExistsInPersistentStore)
                Owner.Directory.AddPersistedClovenSegment(new SubGridCellPassesDataSegmentInfo(
                    CleavingSegment.SegmentInfo.StartTime, OldEndTime,
                    CleavingSegment));

            // Tidy up, marking both segments as dirty, and not existant in the persitant data store!
            CleavingSegment.Dirty = true;
            CleavingSegment.SegmentInfo.ExistsInPersistentStore = false;

            NewSegment.Dirty = true;
            NewSegment.SegmentInfo.ExistsInPersistentStore = false;

            /*
             TODO: InTRex, this non-static information is only maintained in themutable data grid, segment caching not well defined for it yet
            if (Owner.PresentInCache)
            {
                // Include the new segment in the cache segment tracking
                if (!DataStoreInstance.GridDataCache.SubGridSegmentTouched(NewSegment))
                {
                    // todo SIGLogMessage.PublishNoODS(Self,
                    //    Format('Failed to touch newly created segment in segment cleaving for subgrid %s [%s]', [
                    //        CleavingSegment.Owner.Moniker, CleavingSegment.ToString]), slmcException);
                    return true;
                }
            }
            */

            // Check everything looks kosher by comparing the time range of the cells
            // present in the cloven segments with the time range bounds in the segment
            // information for the segments

            // Determine the actual time range of the passes within the segment
            //{$IFDEF DEBUG}
            //    CleavingSegment.VerifyComputedAndRecordedSegmentTimeRangeBounds;
            //    if CleavingSegment.RequiresCleaving then
            //      SIGLogMessage.PublishNoODS(Self,
            //                                 Format('Info: After cleave first segment %d (%.6f-%.6f) of subgrid %s failed to reduce cell pass count below maximums', {SKIP}
            //                                        [Owner.Directory.SegmentDirectory.IndexOf(CleavingSegment), FOwner.Moniker,
            //                                         CleavingSegment.SegmentInfo.StartTime, CleavingSegment.SegmentInfo.EndTime, FOwner.Moniker]),
            //                                 slmcDebug);
            //
            //    NewSegment.VerifyComputedAndRecordedSegmentTimeRangeBounds;
            //    if NewSegment.RequiresCleaving then
            //      SIGLogMessage.PublishNoODS(Self,
            //                                 Format('Info: New cloven segment %d (%.6f-%.6f) (resulting from cleave) of subgrid %s failed to reduce cell pass count below maximums', {SKIP}
            //                                        [Owner.Directory.SegmentDirectory.IndexOf(NewSegment), FOwner.Moniker,
            //                                         NewSegment.SegmentInfo.StartTime, OldEndTime, FOwner.Moniker]),
            //                                 slmcDebug);
            //{$ENDIF}

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
