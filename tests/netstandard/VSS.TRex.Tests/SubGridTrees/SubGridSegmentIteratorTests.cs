using System;
using VSS.TRex.Storage;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Factories;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Iterators;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
  public class SubGridSegmentIteratorTests : IClassFixture<DILoggingAndStorgeProxyFixture>
    {
        [Fact()]
        public void Test_SubGridSegmentIterator_SubGridSegmentIterator()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            IServerLeafSubGrid leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            SubGridSegmentIterator iterator = new SubGridSegmentIterator(leaf, leaf.Directory, StorageProxy.Instance(StorageMutability.Mutable));

            Assert.True(iterator.Directory == leaf.Directory &&
                iterator.SubGrid == leaf, "SubGrid segment iterator not correctly initialised");
        }

        [Fact()]
        public void Test_SubGridSegmentIterator_SetTimeRange()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            IServerLeafSubGrid leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            SubGridSegmentIterator iterator = new SubGridSegmentIterator(leaf, leaf.Directory, StorageProxy.Instance(StorageMutability.Mutable));

            DateTime start = new DateTime(2000, 1, 1, 1, 1, 1);
            DateTime end = new DateTime(2000, 1, 2, 1, 1, 1);
            iterator.SetTimeRange(start, end);

            Assert.True(iterator.IterationState.StartSegmentTime == start && iterator.IterationState.EndSegmentTime == end,
                "Start and end time not set correctly");
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_SubGridSegmentIterator_MoveNext()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_SubGridSegmentIterator_MoveToFirstSubGridSegment()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_SubGridSegmentIterator_MoveToNextSubGridSegment()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_SubGridSegmentIterator_CurrentSubgridSegmentDestroyed()
        {
            Assert.True(false);
        }

        [Fact()]
        public void Test_SubGridSegmentIterator_InitialiseIterator()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            IServerLeafSubGrid leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            SubGridSegmentIterator iterator = new SubGridSegmentIterator(leaf, leaf.Directory,
              StorageProxy.Instance(StorageMutability.Mutable))
            {
                IterationDirection = IterationDirection.Forwards
            };

            iterator.InitialiseIterator();
            Assert.Equal(-1, iterator.IterationState.Idx);

            iterator.IterationDirection = IterationDirection.Backwards;
            iterator.InitialiseIterator();

            Assert.Equal(iterator.IterationState.Idx, leaf.Directory.SegmentDirectory.Count);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_SubGridSegmentIterator_SegmentListExtended()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_SubGridSegmentIterator_MarkCacheStamp()
        {
            Assert.True(false);
        }

        [Fact()]
        public void Test_SubGridSegmentIterator_SetIteratorElevationRange()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            IServerLeafSubGrid leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            SubGridSegmentIterator iterator = new SubGridSegmentIterator(leaf, leaf.Directory, StorageProxy.Instance(StorageMutability.Mutable));

            const double lowerElevation = 9.0;
            const double upperElevation = 19.0;

            iterator.SetIteratorElevationRange(lowerElevation, upperElevation);

            Assert.True(iterator.IterationState.MinIterationElevation == lowerElevation && iterator.IterationState.MaxIterationElevation == upperElevation,
                "Elevation lower and upper bounds not set correctly");
        }
    }
}
