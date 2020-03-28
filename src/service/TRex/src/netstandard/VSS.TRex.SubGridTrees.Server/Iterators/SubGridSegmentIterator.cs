using System;
using System.Collections;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Iterators
{
    /// <summary>
    /// This supports the idea of iterating through the segments in a sub grid in a way that hides all the details
    /// of how this is done...
    /// </summary>
    public class SubGridSegmentIterator : ISubGridSegmentIterator
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridSegmentIterator>();

        // IterationState records the progress of the iteration by recording the path through
        // the sub grid tree which marks the progress of the iteration
        public IIteratorStateIndex IterationState { get; } = new IteratorStateIndex();

        public IStorageProxy StorageProxy { get; set; }

        public bool RetrieveLatestData { get; set; } = false;
        public bool RetrieveAllPasses { get; set; } = true;

        private ISubGridCellPassesDataSegment LocateNextSubGridSegmentInIteration()
        {
            ISubGridCellPassesDataSegment result = null;

            if (IterationState.SubGrid == null)
            {
                Log.LogCritical("No sub grid node assigned to iteration state");
                return null;
            }

            while (IterationState.NextSegment())
            {
                var segmentInfo = IterationState.Directory.SegmentDirectory[IterationState.Idx];

                if (segmentInfo.Segment != null)
                {
                    result = segmentInfo.Segment;
                }

                // If there is no segment present in the cache then it can't be dirty, so is
                // not a candidate to be returned by the iterator
                // Similarly if the caller is only interested in segments that are present in the cache,
                // we do not need to read it from the persistent store
                if (!ReturnDirtyOnly && !ReturnCachedItemsOnly)
                {
                    // This additional check to determine if the segment is defined
                    // is necessary to check if an earlier thread through this code has
                    // already allocated the new segment
                    if (segmentInfo.Segment == null)
                      IterationState.SubGrid.AllocateSegment(segmentInfo);
               
                    result = segmentInfo.Segment;
               
                    if (result == null)
                      throw new TRexSubGridProcessingException("IterationState.SubGrid.Cells.AllocateSegment failed to create a new segment");
                }

                if (result != null)
                { 
                    if (!result.Dirty && ReturnDirtyOnly)
                    {
                        // The segment is not dirty, and the iterator has been instructed only to return
                        // dirty segments, so ignore this one
                        result = null;
                        continue;
                    }
                
                    if (!result.Dirty && !ReturnCachedItemsOnly && 
                        (RetrieveAllPasses && !result.HasAllPasses || RetrieveLatestData && !result.HasLatestData))
                    {
                        if (((IServerSubGridTree) IterationState.SubGrid.Owner).LoadLeafSubGridSegment
                            (StorageProxy,
                             new SubGridCellAddress(IterationState.SubGrid.OriginX, IterationState.SubGrid.OriginY),
                             RetrieveLatestData, RetrieveAllPasses, // StorageClasses,
                             IterationState.SubGrid,
                             result))
                        {
                            /* Raptor Implementation. TRex has no separate cache - it is in Ignite

                            // The segment is now loaded and available for use and should be touched
                            // to link it into the cache segment MRU management
                            if (Result != null && Result.Owner.PresentInCache)
                            {
                               DataStoreInstance.GridDataCache.SubGridSegmentTouched(Result);
                            }
                            */
                        }
                        else
                        {
                            /* Raptor Implementation. TRex has no separate cache - it is in Ignite

                            // The segment is failed to load, however it may have been created
                            // to hold the data being read. The failure handling will have backtracked
                            // out any allocations made within the segment, but it is safer to include
                            // it into the cache and allow it to be managed there than to
                            // independently remove it here
                            if (Result != null && Result.Owner.PresentInCache)
                            {
                                DataStoreInstance.GridDataCache.SubGridSegmentTouched(Result);
                            }
                            */
                
                            // Segment failed to be loaded. Multiple messages will have been posted to the log.
                            // Move to the next item in the iteration
                            result = null; 
                            continue;
                        }
                    }
                }

                if (result != null) // We have a candidate to return as the next item in the iteration
                {                    
                    break;
                }
            }
            return result;
        }

        // CurrentSubGridSegment is a reference to the current sub grid segment that the iterator is currently
        // up to in the sub grid tree scan. 

        public ISubGridCellPassesDataSegment CurrentSubGridSegment { get; set; }

        // property StorageClasses : TICSubGridCellStorageClasses read FStorageClasses write FStorageClasses;

        /// <summary>
        /// ReturnDirtyOnly allows the iterator to only return segments in the sub grid that are dirty
        /// </summary>
        public bool ReturnDirtyOnly { get; set; }

        public IterationDirection IterationDirection { get => IterationState.IterationDirection;  set => IterationState.IterationDirection = value; }

        /// <summary>
        /// Allows the caller of the iterator to restrict the
        /// iteration to the items that are currently in the cache.
        /// </summary>
        public bool ReturnCachedItemsOnly { get; set; } = false;

        /// <summary>
        ///  The sub grid whose segments are being iterated across
        /// </summary>
        public IServerLeafSubGrid SubGrid
        {
            get => IterationState.SubGrid;            
            set => IterationState.SubGrid = value;
        }

        public ISubGridDirectory Directory
        {
          get => IterationState.Directory;
          set => IterationState.Directory = value;
        }

        public bool MarkReturnedSegmentsAsTouched { get; set; }

        private int _numberOfSegmentsScanned;

        public int NumberOfSegmentsScanned
        {
          get => _numberOfSegmentsScanned;
          set => _numberOfSegmentsScanned = value;
        }

        public SubGridSegmentIterator(IServerLeafSubGrid subGrid, IStorageProxy storageProxy)
        {
            MarkReturnedSegmentsAsTouched = true;
            SubGrid = subGrid;
            Directory = subGrid?.Directory;
            StorageProxy = storageProxy;
        }

        public SubGridSegmentIterator(IServerLeafSubGrid subGrid, ISubGridDirectory directory, IStorageProxy storageProxy) : this(subGrid, storageProxy)
        {
            Directory = directory;
        }

        public void SetTimeRange(DateTime startTime, DateTime endTime) => IterationState.SetTimeRange(startTime, endTime);

        public bool MoveNext()
        {
          var result = CurrentSubGridSegment == null ? MoveToFirstSubGridSegment() : MoveToNextSubGridSegment();

          if (CurrentSubGridSegment != null && CurrentSubGridSegment.PassesData == null)
          {
            if (RetrieveAllPasses)
              Log.LogError("Segment retrieved by iterator requiring all cell passes returned a segment with null cell passes");
          }

          return result;
        }

        // MoveToFirstSubGridSegment moves to the first segment in the sub grid
        public bool MoveToFirstSubGridSegment()
        {
            _numberOfSegmentsScanned = 0;

            InitialiseIterator();

            return MoveToNextSubGridSegment();
        }

        // MoveToNextSubGridSegment moves to the next segment in the sub grid
        public bool MoveToNextSubGridSegment()
        {
            var subGridSegment = LocateNextSubGridSegmentInIteration();

            if (subGridSegment == null) // We are at the end of the iteration
            {
                CurrentSubGridSegment = null;
                return false;
            }

            CurrentSubGridSegment = subGridSegment;

            _numberOfSegmentsScanned++;

            return true;
        }

        public void InitialiseIterator() => IterationState.Initialise();

        public bool IsFirstSegmentInTimeOrder => IterationState.Idx == 0;

        // SegmentListExtended advises the iterator that the segment list has grown to include
        // new segments. The meaning of this operation is that the segment the iterator is
        // currently pointing at is still valid, but some number of segments have been
        // inserted into the list of segments. The iterator should continue from the current
        // iterator location in the list, but note the additional number of segments in the list.
        public void SegmentListExtended() => IterationState.SegmentListExtended();

        public int CurrentSegmentIndex => IterationState.Idx;

        public void SetIteratorElevationRange(double minElevation, double maxElevation) => IterationState.SetIteratorElevationRange(minElevation, maxElevation);

        public void SetMachineRestriction(BitArray machineIdSet) => IterationState.SetMachineRestriction(machineIdSet);
    }
}
