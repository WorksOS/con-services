using System;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces
{
    public interface ISubGridSegmentIterator
    {
        int CurrentSegmentIndex { get; }
        SubGridCellPassesDataSegment CurrentSubGridSegment { get; set; }
        SubGridDirectory Directory { get; set; }
        bool IsFirstSegmentInTimeOrder { get; }
        IterationDirection IterationDirection { get; set; }
        IteratorStateIndex IterationState { get; set; }
        bool MarkReturnedSegmentsAsTouched { get; set; }
        int NumberOfSegmentsScanned { get; set; }
        bool RetrieveAllPasses { get; set; }
        bool RetrieveLatestData { get; set; }
        bool ReturnCachedItemsOnly { get; set; }
        bool ReturnDirtyOnly { get; set; }
//        SiteModel SiteModelReference { get; set; }
        ServerSubGridTreeLeaf SubGrid { get; set; }

        void CurrentSubgridSegmentDestroyed();
        void InitialiseIterator();
        void MarkCacheStamp();
        bool MoveNext();
        bool MoveToFirstSubGridSegment();
        bool MoveToNextSubGridSegment();
        void SegmentListExtended();
        void SetIteratorElevationRange(double minElevation, double maxElevation);
        void SetTimeRange(DateTime startTime, DateTime endTime);
    }
}