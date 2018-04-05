using System;
using System.Linq;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    public class SubGridCellPassesDataWrapper
    {
        public IServerLeafSubGrid Owner { get; set; }   // FOwner : TICServerSubGridTreeLeaf;

        public void Clear()
        {
            PassesData.Clear();
        }

        public SubGridCellPassesDataSegments PassesData { get; set; } = new SubGridCellPassesDataSegments();

        public SubGridCellPassesDataWrapper()
        {
            Clear();
        }

        public void Initialise()
        {
            Clear();
        }

        public SubGridCellPassesDataSegment SelectSegment(DateTime time)
        {
            int Index;
            int DirectionIncrement;

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

            Index = 0;
            DirectionIncrement = 0;

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
        public bool CleaveSegment(SubGridCellPassesDataSegment CleavingSegment)
        {
            return false;
        }

        public bool MergeSegments(SubGridCellPassesDataSegment MergeToSegment,
                                  SubGridCellPassesDataSegment MergeFromSegment)
        {
            return false;
        }

        public void RemoveSegment(SubGridCellPassesDataSegment Segment)
        {
        }
    }
}
