using System;
using System.Collections;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
    public interface ISubGridSegmentIterator
    {
        int CurrentSegmentIndex { get; }
        ISubGridCellPassesDataSegment CurrentSubGridSegment { get; set; }
        ISubGridDirectory Directory { get; set; }
        bool IsFirstSegmentInTimeOrder { get; }
        IterationDirection IterationDirection { get; set; }
        IIteratorStateIndex IterationState { get; set; }
        bool MarkReturnedSegmentsAsTouched { get; set; }
        int NumberOfSegmentsScanned { get; set; }
        bool RetrieveAllPasses { get; set; }
        bool RetrieveLatestData { get; set; }
        bool ReturnCachedItemsOnly { get; set; }
        bool ReturnDirtyOnly { get; set; }
        //        SiteModel SiteModelReference { get; set; }
        IServerLeafSubGrid SubGrid { get; set; }

        void CurrentSubgridSegmentDestroyed();
        void InitialiseIterator();
        bool MoveNext();
        bool MoveToFirstSubGridSegment();
        bool MoveToNextSubGridSegment();
        void SegmentListExtended();
        void SetIteratorElevationRange(double minElevation, double maxElevation);
        void SetTimeRange(DateTime startTime, DateTime endTime);
        void SetMachineRestriction(BitArray machineIDSet);
    }
}
