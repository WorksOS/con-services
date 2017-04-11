using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators.Tests
{
    [TestClass()]
    public class SubGridSegmentCellPassIterator_NonStaticTests
    {
        [TestMethod()]
        public void Test_SubGridSegmentCellPassIterator_NonStaticTests_Creation()
        {
            SubGridSegmentCellPassIterator_NonStatic iterator = new SubGridSegmentCellPassIterator_NonStatic();

            Assert.IsTrue(iterator.CellX == byte.MaxValue && iterator.CellY == byte.MaxValue, "CellX/Y not initialised correctly");
            Assert.IsTrue(iterator.MaxNumberOfPassesToReturn == int.MaxValue, "MaxNumberOfPassesToReturn not initialised correctly");
            Assert.IsTrue(iterator.SegmentIterator == null, "SegmentIterator not initialised correctly");
        }

        [TestMethod()]
        public void Test_SubGridSegmentCellPassIterator_NonStaticTests_SetCellCoordinatesInSubgrid()
        {
            SubGridSegmentCellPassIterator_NonStatic iterator = new SubGridSegmentCellPassIterator_NonStatic();

            iterator.SetCellCoordinatesInSubgrid(12, 23);

            Assert.IsTrue(iterator.CellX == 12 && iterator.CellY == 23, "CellX/Y not set correctly");
        }

        [TestMethod()]
        public void Test_SubGridSegmentCellPassIterator_NonStaticTests_SetIteratorElevationRange()
        {
            var tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            var iterator = new SubGridSegmentCellPassIterator_NonStatic(new SubGridSegmentIterator(new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels), new SubGridDirectory()));

            iterator.SetIteratorElevationRange(12.0, 23.0);

            Assert.IsTrue(iterator.SegmentIterator.IterationState.MinIterationElevation == 12.0 &&
                          iterator.SegmentIterator.IterationState.MaxIterationElevation == 23.0, "CellX/Y not set correctly");
        }

        [TestMethod()]
        public void Test_SubGridSegmentCellPassIterator_NonStaticTests_Initialise()
        {
            var tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            var iterator = new SubGridSegmentCellPassIterator_NonStatic(new SubGridSegmentIterator(new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels), new SubGridDirectory()));

            iterator.SegmentIterator.IterationDirection = IterationDirection.Forwards;
            iterator.Initialise();

            iterator.SegmentIterator.IterationDirection = IterationDirection.Backwards;
            iterator.Initialise();
        }

        [TestMethod()]
        public void Test_SubGridSegmentCellPassIterator_NonStaticTests_GetNextCellPass()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_SubGridSegmentCellPassIterator_NonStaticTests_MayHaveMoreFilterableCellPasses()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_SubGridSegmentCellPassIterator_NonStaticTests_SetTimeRangeTest()
        {
            var tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            var iterator = new SubGridSegmentCellPassIterator_NonStatic(new SubGridSegmentIterator(new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels), new SubGridDirectory()));

            iterator.SetTimeRange(true, new DateTime(2000, 1, 1), new DateTime(2000, 1, 2));

            Assert.IsTrue(iterator.IteratorStartTime == new DateTime(2000, 1, 1) && iterator.IteratorEndTime == new DateTime(2000, 1, 2),
                          "Iteration start and end date not set correctly");
        }
    }
}