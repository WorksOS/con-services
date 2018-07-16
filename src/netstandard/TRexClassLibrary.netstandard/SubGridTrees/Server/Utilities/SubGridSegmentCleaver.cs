using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Iterators;

namespace VSS.TRex.SubGridTrees.Server.Utilities
{
    /// <summary>
    /// Provides segment cleaving semantics against the set of segments contained within a subgrid
    /// </summary>
    public static class SubGridSegmentCleaver
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        public static bool RecordSegmentCleavingOperationsToLog = true;

        /// <summary>
        /// Cleaves all dirty segments requiring cleaving within the given subgrid
        /// </summary>
        /// <param name="storageProxy"></param>
        /// <param name="subGrid"></param>
        public static void PerformSegmentCleaving(IStorageProxy storageProxy, IServerLeafSubGrid subGrid)
        {
            // TODO Need to determine locking semantics governing this
            // Debug.Assert(subGrid.Locked, "May not calculate latest pass information if the subgrid is not locked");

            SubGridSegmentIterator Iterator = new SubGridSegmentIterator(subGrid, storageProxy)
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
                            Log.LogInformation($"Info: Performed cleave on segment ({CleavedTimeRangeStart}-{CleavedTimeRangeEnd}) of subgrid {ServerSubGridTree.GetLeafSubGridFullFileName(Origin)}");
                    }
                    else
                    {
                        // The segment cleave failed. Currently the only cause of this is a
                        // database modification lock acquisition failure. While this is not
                        // a serious problem (as the subgrid will be cleaved at some point in
                        // the future when it is modified again via tag file proccessing etc)
                        // it will be noted in the log.

                        if (RecordSegmentCleavingOperationsToLog)
                            Log.LogInformation($"Info: Cleave on segment ({CleavedTimeRangeStart}-{CleavedTimeRangeEnd}) of subgrid {ServerSubGridTree.GetLeafSubGridFullFileName(Origin)} failed");
                    }
                }

                if (RecordSegmentCleavingOperationsToLog)
                {
                    foreach (var segment in subGrid.Cells.PassesData.Items)
                    {
                        if (Segment.RequiresCleaving())
                        {
                            SegmentTotalPassesCalculator.CalculateTotalPasses(segment.PassesData,out uint TotalPassCount, out uint MaximumPassCount);
                            Log.LogInformation($"Info: Cleave on segment ({CleavedTimeRangeStart}-{CleavedTimeRangeEnd}) of subgrid {subGrid.Moniker()} failed to reduce cell pass count below maximums (max passes = {TotalPassCount}/{TRexConfig.VLPD_SubGridSegmentPassCountLimit}, per cell = {MaximumPassCount}/{TRexConfig.VLPD_SubGridMaxSegmentCellPassesLimit})");
                        }
                    }
                }
            } while (Iterator.MoveToNextSubGridSegment());
        }
    }
}
