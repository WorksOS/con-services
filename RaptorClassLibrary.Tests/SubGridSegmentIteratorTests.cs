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
    public class SubGridSegmentIteratorTests
    {
        [TestMethod()]
        public void Test_SubGridSegmentIterator_SubGridSegmentIterator()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            SubGridSegmentIterator iterator = new SubGridSegmentIterator(leaf, leaf.Directory);

            Assert.IsTrue(iterator.Directory == leaf.Directory &&
                iterator.SubGrid == leaf, "SubGrid segment iterator not correctly initialised");
        }

        [TestMethod()]
        public void Test_SubGridSegmentIterator_SetTimeRange()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            SubGridSegmentIterator iterator = new SubGridSegmentIterator(leaf, leaf.Directory);

            DateTime start = new DateTime(2000, 1, 1, 1, 1, 1);
            DateTime end = new DateTime(2000, 1, 2, 1, 1, 1);
            iterator.SetTimeRange(start, end);

            Assert.IsTrue(iterator.IterationState.StartSegmentTime == start && iterator.IterationState.EndSegmentTime == end,
                "Start and end time not set correctly");
        }

        [TestMethod()]
        public void Test_SubGridSegmentIterator_MoveNext()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_SubGridSegmentIterator_MoveToFirstSubGridSegment()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_SubGridSegmentIterator_MoveToNextSubGridSegment()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_SubGridSegmentIterator_CurrentSubgridSegmentDestroyed()
        {
            Assert.Inconclusive();
        }

        [TestMethod()]
        public void Test_SubGridSegmentIterator_InitialiseIterator()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            SubGridSegmentIterator iterator = new SubGridSegmentIterator(leaf, leaf.Directory);

            iterator.IterationDirection = IterationDirection.Forwards;
            iterator.InitialiseIterator();
            Assert.IsTrue(iterator.IterationState.Idx == -1, "Iterator state Idx is not -1 after initialisation with moving forwards");

            iterator.IterationDirection = IterationDirection.Backwards;
            iterator.InitialiseIterator();

            Assert.IsTrue(iterator.IterationState.Idx == leaf.Directory.SegmentDirectory.Count(), "Iterator state Idx is not SegmentDirectory.Count() after initialisation with moving backwards");
        }

        [TestMethod()]
        public void Test_SubGridSegmentIterator_SegmentListExtended()
        {
            Assert.Inconclusive();
        }

        [TestMethod()]
        public void Test_SubGridSegmentIterator_MarkCacheStamp()
        {
            Assert.Inconclusive();
        }

        [TestMethod()]
        public void Test_SubGridSegmentIterator_SetIteratorElevationRange()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            SubGridSegmentIterator iterator = new SubGridSegmentIterator(leaf, leaf.Directory);

            double lowerElevation = 9.0;
            double upperElevation = 19.0;

            iterator.SetIteratorElevationRange(lowerElevation, upperElevation);

            Assert.IsTrue(iterator.IterationState.MinIterationElevation == lowerElevation && iterator.IterationState.MaxIterationElevation == upperElevation,
                "Elevation lower and upper bounds not set correctly");
        }
    }
}