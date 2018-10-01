using System;
using System.Collections.Generic;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Iterators;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
    public class SubgridSegmentCleavingTests_NonStatic : IClassFixture<DILoggingFixture>
  {
        private static DateTime startTime = new DateTime(2000, 1, 1, 0, 0, 0);

        private static IServerLeafSubGrid MakeSubgridWith10240CellPassesAtOneSecondIntervals()
        {
            IServerLeafSubGrid subGrid = new ServerSubGridTreeLeaf();
            subGrid.AllocateLeafFullPassStacks();

            // Create a segment with 10240 cell passes with each cell pass occuring one second apart. 
            DateTime passTime = startTime;

            for (int i = 0; i < 10; i++)
            {
              VSS.TRex.SubGridTrees.Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((x, y) =>
                {
                    subGrid.AddPass(x, y, new CellPass()
                    {
                        Time = passTime,
                        Height = (float)1.0
                    });

                    passTime = passTime.AddSeconds(1);
                });
            }

            return subGrid;
        }

        [Fact()]
        public void MetaTest_Test_MakeSubgridWith10240CellPassesAtOneSecondIntervals()
        {
            // Create a subgrid to hold the segment
            IServerLeafSubGrid subGrid = MakeSubgridWith10240CellPassesAtOneSecondIntervals();

            ISubGridCellPassesDataSegment segment = subGrid.Cells.PassesData[0];

      // Check all cells have exactly 10 passes
          VSS.TRex.SubGridTrees.Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((x, y) =>
            {
                Assert.True(segment.PassesData.PassCount(x, y) == 10,
                    $"Cell in segment at {x},{y} does not have 10 cell passes");
            });

            //Check the total number of passes is 10240, and the maximum pass count is 10
            segment.PassesData.CalculateTotalPasses(out uint totalPassCount, out uint maximumPassCount);

            Assert.True(10240 == totalPassCount, "Initial total pass count not 10240");
            Assert.True(10240 == segment.PassesData.SegmentPassCount,
                $"segment.PassesData.SegmentPassCount does not equal 10240 (it is {segment.PassesData.SegmentPassCount})");
            Assert.True(10 == maximumPassCount, "Initial maximum pass count not 10");

            // Check the time range is as expected
            segment.PassesData.CalculateTimeRange(out DateTime startSegmentTime, out DateTime endSegmentTime);
            Assert.True(endSegmentTime > startSegmentTime, $"End time {endSegmentTime} not greater than startTime {startSegmentTime}");

            Assert.True(startSegmentTime == startTime, $"Start time {startSegmentTime} not equal to {startTime} as expected");
            Assert.True(endSegmentTime == startTime.AddSeconds(10239), $"End time {endSegmentTime} not equal to {startTime.AddSeconds(10239)} as expected");
        }

        [Fact()]
        public void Test_SegmentIterator_MoveToFirstSegment()
        {
            IServerLeafSubGrid subGrid = MakeSubgridWith10240CellPassesAtOneSecondIntervals();

            SubGridSegmentIterator Iterator = new SubGridSegmentIterator(subGrid, StorageProxy.Instance(StorageMutability.Mutable))
            {
                IterationDirection = IterationDirection.Forwards,
                ReturnDirtyOnly = true,
                RetrieveAllPasses = true
            };

            Assert.True(!Iterator.MoveToFirstSubGridSegment(), "Was able to move to first segment (forwards) when requesting only dirty segments");

            Iterator = new SubGridSegmentIterator(subGrid, StorageProxy.Instance(StorageMutability.Mutable))
            {
                IterationDirection = IterationDirection.Backwards,
                ReturnDirtyOnly = true,
                RetrieveAllPasses = true
            };

            Assert.True(!Iterator.MoveToFirstSubGridSegment(), "Was able to move to first segment (backwards) when requesting only dirty segments");

            Iterator = new SubGridSegmentIterator(subGrid, StorageProxy.Instance(StorageMutability.Mutable))
            {
                IterationDirection = IterationDirection.Forwards,
                ReturnDirtyOnly = false,
                RetrieveAllPasses = true
            };

            Assert.True(Iterator.MoveToFirstSubGridSegment(), "Was not able to move to first segment (forwards) when requesting all segments");

            Iterator = new SubGridSegmentIterator(subGrid, StorageProxy.Instance(StorageMutability.Mutable))
            {
                IterationDirection = IterationDirection.Backwards,
                ReturnDirtyOnly = false,
                RetrieveAllPasses = true
            };

            Assert.True(Iterator.MoveToFirstSubGridSegment(), "Was not able to move to first segment (backwards) when requesting all segments");
        }

        [Fact()]
        public void Test_SubgridSegmentCleaving_CellPassAddition_IncreasingTime()
        {
            IServerLeafSubGrid subGrid = new ServerSubGridTreeLeaf();
            subGrid.AllocateLeafFullPassStacks();

            // Create a segment with 10240 cell passes with each cell pass occuring one second apart. 
            DateTime passTime = startTime;

            subGrid.AddPass(0, 0, new CellPass()
            {
                Time = passTime,
                Height = (float)1.0
            });

            passTime = passTime.AddSeconds(1);

            subGrid.AddPass(0, 0, new CellPass()
            {
                Time = passTime,
                Height = (float)1.0
            });

            Assert.True(2 == subGrid.Cells.PassesData[0].PassesData.PassCount(0, 0), $"Number of cells not 2 as expected (= {subGrid.Cells.PassesData[0].PassesData.PassCount(0, 0)}");

            Assert.True(subGrid.Cells.PassesData[0].PassesData.PassTime(0, 0, 0) < subGrid.Cells.PassesData[0].PassesData.PassTime(0, 0, 1),
                $"The two passes added are not in expected order: 1 => {subGrid.Cells.PassesData[0].PassesData.PassTime(0, 0, 0)}, 2 => {subGrid.Cells.PassesData[0].PassesData.PassTime(0, 0, 1)}");
        }

        [Fact()]
        public void Test_SubgridSegmentCleaving_CellPassAddition_DecreasingTime()
        {
            IServerLeafSubGrid subGrid = new ServerSubGridTreeLeaf();
            subGrid.AllocateLeafFullPassStacks();

            // Create a segment with 10240 cell passes with each cell pass occuring one second apart. 
            DateTime passTime = startTime.AddSeconds(1);

            subGrid.AddPass(0, 0, new CellPass()
            {
                Time = passTime,
                Height = (float)1.0
            });

            passTime = passTime.AddSeconds(-1);

            subGrid.AddPass(0, 0, new CellPass()
            {
                Time = passTime,
                Height = (float)1.0
            });

            Assert.True(2 == subGrid.Cells.PassesData[0].PassesData.PassCount(0, 0), $"Number of cells not 2 as expected (= {subGrid.Cells.PassesData[0].PassesData.PassCount(0, 0)}");

            Assert.True(subGrid.Cells.PassesData[0].PassesData.PassTime(0, 0, 0) < subGrid.Cells.PassesData[0].PassesData.PassTime(0, 0, 1),
                $"The two passes added are not in expected order: 1 => {subGrid.Cells.PassesData[0].PassesData.PassTime(0, 0, 0)}, 2 => {subGrid.Cells.PassesData[0].PassesData.PassTime(0, 0, 1)}");
        }

        [Fact()]
        public void Test_SubgridSegmentCleaving_AdoptCellPassesFrom()
        {
            // Create the subgrid with lots of cell passes
            IServerLeafSubGrid subGrid = MakeSubgridWith10240CellPassesAtOneSecondIntervals();
            ISubGridCellPassesDataSegment segment1 = subGrid.Cells.PassesData[0];

            // Get the time range
            segment1.PassesData.CalculateTimeRange(out DateTime startSegmentTime, out DateTime endSegmentTime);

            // Create a second segment specially and use the cell pass adopter to move cell passes 
            SubGridCellPassesDataSegment segment2 = new SubGridCellPassesDataSegment
            {
                SegmentInfo = new SubGridCellPassesDataSegmentInfo()
            };
            segment2.AllocateFullPassStacks();

            SegmentCellPassAdopter.AdoptCellPassesFrom(segment2.PassesData, segment1.PassesData, new DateTime((startSegmentTime.Ticks + endSegmentTime.Ticks) / 2));

            // Check the times of the adopted cells are correct
            Assert.True(segment1.VerifyComputedAndRecordedSegmentTimeRangeBounds(), "Segment1 has inappropriate cell pass time range compared to segment time range");
            Assert.True(segment2.VerifyComputedAndRecordedSegmentTimeRangeBounds(), "Segment2 has inappropriate cell pass time range compared to segment time range");

            segment1.PassesData.CalculateTotalPasses(out uint totalPassCount1, out uint maximumPassCount1);
            segment2.PassesData.CalculateTotalPasses(out uint totalPassCount2, out uint maximumPassCount2);

            Assert.True(10240 == (totalPassCount1 + totalPassCount2), $"Totals ({totalPassCount1} and {totalPassCount2} don't add up to 10240 after cleaving");
            Assert.True(5 == maximumPassCount1, $"Maximum pass count 1 {maximumPassCount1}, is not 5");
            Assert.True(5 == maximumPassCount2, $"Maximum pass count 2 {maximumPassCount2}, is not 5");

            // Check the segment pass count in the segment is correct
            Assert.True(totalPassCount1 == segment1.PassesData.SegmentPassCount, $"Total passes for segment 1 {totalPassCount1} is not equal to segmentPassCount in that segment {segment1.PassesData.SegmentPassCount}");
            Assert.True(totalPassCount2 == segment2.PassesData.SegmentPassCount, $"Total passes for segment 2 {totalPassCount2} is not equal to segmentPassCount in that segment {segment2.PassesData.SegmentPassCount}");
        }

        [Fact()]
        public void Test_SubgridSegmentCleaving()
        {
            // Create a subgrid to hold the segment
            IServerLeafSubGrid subGrid = MakeSubgridWith10240CellPassesAtOneSecondIntervals();

            ISubGridCellPassesDataSegment segment = subGrid.Cells.PassesData[0];

            // Instruct the segment container to cleave the segment
            // Modify the cleaving limit to 100000 to force the segment not to be cloven = the cleave result should be false
            TRexConfig.VLPD_SubGridSegmentPassCountLimit = 100000;
            var persistedClovenSegments = new List<ISubGridSpatialAffinityKey>();
            Assert.False(subGrid.Cells.CleaveSegment(segment, persistedClovenSegments), "Segment was cloven when cell pass count was below limit");

            // Modify the cleaving limit to 10000 to force the segment not to be cloven = the cleave result should be true
            TRexConfig.VLPD_SubGridSegmentPassCountLimit = 10000;
            persistedClovenSegments = new List<ISubGridSpatialAffinityKey>();
            Assert.True(subGrid.Cells.CleaveSegment(segment, persistedClovenSegments), "Segment failed to cleave with pass count above limit");

            //Check there are now two segments in total
            Assert.True(2 == subGrid.Cells.PassesData.Count, $"After cleaving there are {subGrid.Cells.PassesData.Count} segments instead of the expected two segments");

            //Check the total number of passes across the two segments is 10240, and the maximum pass count is 5
            ISubGridCellPassesDataSegment segment1 = subGrid.Cells.PassesData[0];
            ISubGridCellPassesDataSegment segment2 = subGrid.Cells.PassesData[1];

            segment1.PassesData.CalculateTotalPasses(out uint totalPassCount1, out uint maximumPassCount1);
            segment2.PassesData.CalculateTotalPasses(out uint totalPassCount2, out uint maximumPassCount2);

            Assert.True(10240 == (totalPassCount1 + totalPassCount2), $"Totals ({totalPassCount1} and {totalPassCount2} don't add up to 10240 after cleaving");
            Assert.True(5 == maximumPassCount1, $"Maximum pass count 1 {maximumPassCount1}, is not 5");
            Assert.True(5 == maximumPassCount2, $"Maximum pass count 2 {maximumPassCount2}, is not 5");

            // Check the segment pass count in the segment is correct
            Assert.True(totalPassCount1 == segment1.PassesData.SegmentPassCount, $"Total passes for segment 1 {totalPassCount1} is not equal to segmentPassCount in that segment {segment1.PassesData.SegmentPassCount}");
            Assert.True(totalPassCount2 == segment2.PassesData.SegmentPassCount, $"Total passes for segment 2 {totalPassCount2} is not equal to segmentPassCount in that segment {segment2.PassesData.SegmentPassCount}");
        }

        [Fact()]
        public void Test_SubgridSegment_Cleaver()
        {
            // Create a subgrid to hold the segment
            IServerLeafSubGrid subGrid = MakeSubgridWith10240CellPassesAtOneSecondIntervals();

            // Modify the cleaving limit to 10000 to force the segment not to be cloven = the cleave result should be true
            TRexConfig.VLPD_SubGridSegmentPassCountLimit = 10000;

            // Exercise the cleaver!
            // Instruct the segment container to cleave the segment
            // Set the segment to not dirty - it should be ignored
            subGrid.Cells.PassesData[0].Dirty = false;

            var cleaver = new SubGridSegmentCleaver();
            cleaver.PerformSegmentCleaving(StorageProxy.Instance(StorageMutability.Mutable), subGrid);

            Assert.True(1 == subGrid.Cells.PassesData.Count, $"After cleaving with no dirty segments there are {subGrid.Cells.PassesData.Count} segments instead of the expected one segments");

            // Set the segment to not dirty - it should be ignored
            subGrid.Cells.PassesData[0].Dirty = true;
            cleaver = new SubGridSegmentCleaver();
            cleaver.PerformSegmentCleaving(StorageProxy.Instance(StorageMutability.Mutable), subGrid);

            //Check there are now two segments in total
            Assert.True(2 == subGrid.Cells.PassesData.Count, $"After cleaving there are {subGrid.Cells.PassesData.Count} segments instead of the expected two segments");

            //Check the total number of passes across the two segments is 10240, and the maximum pass count is 5
            ISubGridCellPassesDataSegment segment1 = subGrid.Cells.PassesData[0];
            ISubGridCellPassesDataSegment segment2 = subGrid.Cells.PassesData[1];

            segment1.PassesData.CalculateTotalPasses(out uint totalPassCount1, out uint maximumPassCount1);
            segment2.PassesData.CalculateTotalPasses(out uint totalPassCount2, out uint maximumPassCount2);

            Assert.True(10240 == (totalPassCount1 + totalPassCount2), $"Totals ({totalPassCount1} and {totalPassCount2} don't add up to 10240 after cleaving");
            Assert.True(5 == maximumPassCount1, $"Maximum pass count 1 {maximumPassCount1}, is not 5");
            Assert.True(5 == maximumPassCount2, $"Maximum pass count 2 {maximumPassCount2}, is not 5");

            // Check the segment pass count in the segment is correct
            Assert.True(totalPassCount1 == segment1.PassesData.SegmentPassCount, $"Total passes for segment 1 {totalPassCount1} is not equal to segmentPassCount in that segment {segment1.PassesData.SegmentPassCount}");
            Assert.True(totalPassCount2 == segment2.PassesData.SegmentPassCount, $"Total passes for segment 2 {totalPassCount2} is not equal to segmentPassCount in that segment {segment2.PassesData.SegmentPassCount}");
        }

    [Fact]
    public void Test_SubgridSegment_VerifyComputedAndRecordedSegmentTimeRangeBounds_Success()
    {
      // Create a subgrid to hold the segment
      IServerLeafSubGrid subGrid = MakeSubgridWith10240CellPassesAtOneSecondIntervals();

      Assert.True(subGrid.Cells.PassesData[0].VerifyComputedAndRecordedSegmentTimeRangeBounds(), "Newly created segment fails bounds test");
    }

    [Fact]
    public void Test_SubgridSegment_VerifyComputedAndRecordedSegmentTimeRangeBounds_Fail()
    {
      // Create a subgrid to hold the segment
      IServerLeafSubGrid subGrid = MakeSubgridWith10240CellPassesAtOneSecondIntervals();
      subGrid.Cells.PassesData[0].SegmentInfo.EndTime = new DateTime(1900, 1, 1);

      Assert.False(subGrid.Cells.PassesData[0].VerifyComputedAndRecordedSegmentTimeRangeBounds(), "Modified invalid segment passes bounds test");
    }

  }
}
