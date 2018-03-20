using VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators.Tests
{
        public class SubGridSegmentCellPassIterator_NonStaticTests
    {
        [Fact()]
        public void Test_SubGridSegmentCellPassIterator_NonStaticTests_Creation()
        {
            SubGridSegmentCellPassIterator_NonStatic iterator = new SubGridSegmentCellPassIterator_NonStatic();

            Assert.True(iterator.CellX == byte.MaxValue && iterator.CellY == byte.MaxValue, "CellX/Y not initialised correctly");
            Assert.Equal(iterator.MaxNumberOfPassesToReturn, int.MaxValue);
            Assert.Null(iterator.SegmentIterator);
        }

        [Fact()]
        public void Test_SubGridSegmentCellPassIterator_NonStaticTests_SetCellCoordinatesInSubgrid()
        {
            SubGridSegmentCellPassIterator_NonStatic iterator = new SubGridSegmentCellPassIterator_NonStatic();

            iterator.SetCellCoordinatesInSubgrid(12, 23);

            Assert.True(iterator.CellX == 12 && iterator.CellY == 23, "CellX/Y not set correctly");
        }

        [Fact()]
        public void Test_SubGridSegmentCellPassIterator_NonStaticTests_SetIteratorElevationRange()
        {
            var tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            var iterator = new SubGridSegmentCellPassIterator_NonStatic(new SubGridSegmentIterator(new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels), new SubGridDirectory()));

            iterator.SetIteratorElevationRange(12.0, 23.0);

            Assert.True(iterator.SegmentIterator.IterationState.MinIterationElevation == 12.0 &&
                          iterator.SegmentIterator.IterationState.MaxIterationElevation == 23.0, "CellX/Y not set correctly");
        }

        [Fact()]
        public void Test_SubGridSegmentCellPassIterator_NonStaticTests_Initialise()
        {
            var tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            var iterator = new SubGridSegmentCellPassIterator_NonStatic(new SubGridSegmentIterator(new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels), new SubGridDirectory()));

            iterator.SegmentIterator.IterationDirection = IterationDirection.Forwards;
            iterator.Initialise();

            iterator.SegmentIterator.IterationDirection = IterationDirection.Backwards;
            iterator.Initialise();
        }

        [Fact()]
        public void Test_SubGridSegmentCellPassIterator_NonStaticTests_GetNextCellPass()
        {
            Assert.True(false);
        }

        [Fact()]
        public void Test_SubGridSegmentCellPassIterator_NonStaticTests_MayHaveMoreFilterableCellPasses()
        {
            Assert.True(false);
        }

        [Fact()]
        public void Test_SubGridSegmentCellPassIterator_NonStaticTests_SetTimeRangeTest()
        {
            var tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            var iterator = new SubGridSegmentCellPassIterator_NonStatic(new SubGridSegmentIterator(new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels), new SubGridDirectory()));

            iterator.SetTimeRange(true, new DateTime(2000, 1, 1), new DateTime(2000, 1, 2));

            Assert.True(iterator.IteratorStartTime == new DateTime(2000, 1, 1) && iterator.IteratorEndTime == new DateTime(2000, 1, 2),
                          "Iteration start and end date not set correctly");
        }
    }
}