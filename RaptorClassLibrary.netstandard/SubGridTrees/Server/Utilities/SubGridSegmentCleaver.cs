using System.Diagnostics;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Utilities
{
    /// <summary>
    /// Provides segment cleaving semantics against the set of segments contained within a subgrid
    /// </summary>
    public static class SubGridSegmentCleaver
    {
        /// <summary>
        /// Cleaves all segments requiring it within the given subgrid
        /// </summary>
        /// <param name="subGrid"></param>
        public static void PerformSegmentCleaving(IServerLeafSubGrid subGrid)
        {
            Debug.Assert(subGrid.Locked, "May not calculate latest pass information if the subgrid is not locked");

            SubGridSegmentIterator Iterator = new SubGridSegmentIterator(subGrid)
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
                SubGridCellPassesDataSegment Segment = Iterator.CurrentSubGridSegment;

                // TODO DateTime CleavedTimeRangeStart = Segment.SegmentInfo.StartTime;
                // TODO DateTime CleavedTimeRangeEnd = Segment.SegmentInfo.EndTime;

                if (Segment.RequiresCleaving())
                {
                    if (subGrid.Cells.CleaveSegment(Segment))
                    {
                        Iterator.SegmentListExtended();

                        /* TODO
                        if (RecordSegmentCleavingOperationsToLog)
                        {
                            SIGLogMessage.PublishNoODS(Self,
                                Format('Info: Performed cleave on segment %d (%.6f-%.6f) of subgrid %s', 
                                [FCells.PassesData.IndexOf(Segment), CleavedTimeRangeStart, CleavedTimeRangeEnd,
                                    ExtractFileName(
                                        (FOwner as TICServerSubGridTree).GetLeafSubGridFullFileName(Origin))]),
                            slmcDebug);
                        }*/
                    }
                    else
                    {
                        // The segment cleave failed. Currently the only cause of this is a
                        // database modification lock acquisition failure. While this is not
                        // a serious problem (as the subgrid will be cleaved at some point in
                        // the future when it is modified again via tag file proccessing etc)
                        // it will be noted in the log.

                        /* TODO
                          SIGLogMessage.Publish(Self,
                                                 Format('Info: Cleave of segment %d (%.6f-%.6f) of subgrid %s failed', 
                                                 [FCells.PassesData.IndexOf(Segment),
                                                  CleavedTimeRangeStart, CleavedTimeRangeEnd, Moniker,
                                                   ExtractFileName((FOwner as TICServerSubGridTree).GetLeafSubGridFullFileName(Origin))]),
                                                   slmcDebug);
                        */
                    }
                }

                /* TODO
        if RecordSegmentCleavingOperationsToLog then
        begin
        for Segment in Iterator do
            if Segment.RequiresCleaving then
        begin
        Segment.CalculateTotalPasses(out int TotalPassCount, out int MaximumPassCount);
        SIGLogMessage.PublishNoODS(Self,
            Format('Info: Cleave of segment %d (%.6f-%.6f) of subgrid %s failed to reduce cell pass count below maximums (max passes = %d/%d, per cell = %d/%d)',
        [FCells.PassesData.IndexOf(Segment),
        CleavedTimeRangeStart, CleavedTimeRangeEnd, Moniker,
        TotalPassCount, VLPDSvcLocations.VLPD_SubGridSegmentPassCountLimit,
        MaximumPassCount, VLPDSvcLocations.VLPD_SubGridMaxSegmentCellPassesLimit]),
        slmcDebug);
        end;
        end;
        */
            } while (Iterator.MoveToNextSubGridSegment());
        }
    }
}
