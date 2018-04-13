using System;
using System.Diagnostics;
using System.Reflection;
using log4net;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Utilities
{
    /// <summary>
    /// Provides segment cleaving semantics against the set of segments contained within a subgrid
    /// </summary>
    public static class SubGridSegmentCleaver
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool RecordSegmentCleavingOperationsToLog = true;

        /// <summary>
        /// Cleaves all dirty segments requiring cleaving within the given subgrid
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

                DateTime CleavedTimeRangeStart = Segment.SegmentInfo.StartTime;
                DateTime CleavedTimeRangeEnd = Segment.SegmentInfo.EndTime;

                if (Segment.RequiresCleaving())
                {
                    if (subGrid.Cells.CleaveSegment(Segment))
                    {
                        Iterator.SegmentListExtended();

                        if (RecordSegmentCleavingOperationsToLog)
                            Log.Info($"Info: Performed cleave on segment ({CleavedTimeRangeStart}-{CleavedTimeRangeEnd}) of subgrid {ServerSubGridTree.GetLeafSubGridFullFileName(Origin)}");
                    }
                    else
                    {
                        // The segment cleave failed. Currently the only cause of this is a
                        // database modification lock acquisition failure. While this is not
                        // a serious problem (as the subgrid will be cleaved at some point in
                        // the future when it is modified again via tag file proccessing etc)
                        // it will be noted in the log.

                        if (RecordSegmentCleavingOperationsToLog)
                            Log.Info($"Info: Cleave on segment ({CleavedTimeRangeStart}-{CleavedTimeRangeEnd}) of subgrid {ServerSubGridTree.GetLeafSubGridFullFileName(Origin)} failed");
                    }
                }

                if (RecordSegmentCleavingOperationsToLog)
                {
                    foreach (var segment in subGrid.Cells.PassesData.Items)
                    {
                        if (Segment.RequiresCleaving())
                        {
                            SegmentTotalPassesCalculator.CalculateTotalPasses(segment.PassesData,out uint TotalPassCount, out uint MaximumPassCount);
                            Log.Info($"Info: Cleave on segment ({CleavedTimeRangeStart}-{CleavedTimeRangeEnd}) of subgrid {subGrid.Moniker()} failed to reduce cell pass count below maximums (max passes = {TotalPassCount}/{RaptorConfig.VLPD_SubGridSegmentPassCountLimit}, per cell = {MaximumPassCount}/{RaptorConfig.VLPD_SubGridMaxSegmentCellPassesLimit})");
                        }
                    }
                }
            } while (Iterator.MoveToNextSubGridSegment());
        }
    }
}
