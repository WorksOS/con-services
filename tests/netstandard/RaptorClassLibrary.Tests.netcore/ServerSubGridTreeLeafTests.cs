using VSS.TRex.SubGridTrees.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.TRex.Cells;
using VSS.TRex.Storage;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.SubGridTrees.Server.Tests
{
        public class ServerSubGridTreeLeafTests
    {
        private CellPass CreateTestCellPass()
        {
            return new CellPass()
            {
                Amplitude = 100,
                CCA = 200,
                CCV = 300,
                Frequency = 500,
                gpsMode = GPSMode.Fixed,
                HalfPass = false,
                Height = (float)123.0,
                //MachineID = 12345,
                InternalSiteModelMachineIndex = 12345,
                MachineSpeed = 234,
                MaterialTemperature = 700,
                MDP = 800,
                PassType = PassType.Front,
                RadioLatency = 10,
                RMV = 900,
                Time = new DateTime(2000, 1, 1, 1, 1, 1)
            };
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_Clear()
        {
            // Add a cell pass and check the CellHasValue flags the cell as having a value
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            IServerLeafSubGrid leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            leaf.AllocateLeafFullPassStacks();
            leaf.CreateDefaultSegment();
            leaf.AllocateFullPassStacks(leaf.Directory.SegmentDirectory.First());
            leaf.AllocateLeafLatestPassGrid();

            CellPass pass = CreateTestCellPass();
            leaf.AddPass(0, 0, pass);
            leaf.ComputeLatestPassInformation(true, StorageProxyFactory.Storage(StorageMutability.Mutable));

            leaf.Clear();

            Assert.False(leaf.CellHasValue(0, 0), "Cell has a value");
            //            Assert.IsTrue(leaf.Directory.SegmentDirectory.First().Segment.PassesData.PassData[0,0].PassCount == 0, "cell pass count is not 0");
            Assert.Null(leaf.Directory.SegmentDirectory.First().Segment);
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_ServerSubGridTreeLeaf()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            IServerLeafSubGrid leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            Assert.True(leaf.Cells == null &&
                leaf.Directory != null &&
                leaf.Dirty == false &&
                leaf.LeafEndTime == DateTime.MinValue &&
                leaf.LeafStartTime == DateTime.MaxValue &&
                leaf.Level == SubGridTree.SubGridTreeLevels &&
                leaf.IsLeafSubGrid() == true,
                "Leaf not initialised as expected");
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_AddPass()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            IServerLeafSubGrid leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            leaf.AllocateLeafFullPassStacks();
            leaf.CreateDefaultSegment();
            leaf.AllocateFullPassStacks(leaf.Directory.SegmentDirectory.First());

            Assert.True(leaf.Cells != null &&
                          leaf.Directory.SegmentDirectory.First().Segment != null &&
                          leaf.Directory.SegmentDirectory.First().Segment.PassesData != null,
                          "Segment passes data not created correctly for AddPass()");

            CellPass pass = CreateTestCellPass();
            leaf.AddPass(0, 0, pass);

            // Check the cell passes in the segment records the cell pass
            Assert.Equal(1, leaf.Directory.SegmentDirectory.First().Segment.PassesData.SegmentPassCount);
            Assert.Equal((uint)1, leaf.Directory.SegmentDirectory.First().Segment.PassesData.PassCount(0, 0));
            Assert.Equal(leaf.Directory.SegmentDirectory.First().Segment.PassesData.PassTime(0, 0, 0), new DateTime(2000, 1, 1, 1, 1, 1));

            // Pull the pass a compare it to what was added
            CellPass pass2 = leaf.Directory.SegmentDirectory.First().Segment.PassesData.Pass(0, 0, 0);
            Assert.True(pass2.Equals(pass), "Pass retrieved is not the same as the pass asses");

            // Check that the start and end time for the leaf was updated when the cell pass was added.
            Assert.True(leaf.LeafStartTime == leaf.LeafEndTime && leaf.LeafStartTime == new DateTime(2000, 1, 1, 1, 1, 1), "Leaf start and time was not updated");
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_CreateDefaultSegment()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            IServerLeafSubGrid leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            Assert.True(0 == leaf.Directory.SegmentDirectory.Count);

            leaf.CreateDefaultSegment();

            Assert.True(1 == leaf.Directory.SegmentDirectory.Count);

            Assert.True(leaf.Directory.SegmentDirectory.First().StartTime == DateTime.MinValue &&
                          leaf.Directory.SegmentDirectory.First().EndTime == DateTime.MaxValue,
                          "Default segment does not have history spanning time range");
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_AllocateFullPassStacks()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            IServerLeafSubGrid leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            leaf.AllocateLeafFullPassStacks();
            leaf.CreateDefaultSegment();
            leaf.AllocateFullPassStacks(leaf.Directory.SegmentDirectory.First());

            Assert.True(leaf.Cells != null &&
                leaf.Directory != null &&
                leaf.Dirty == false &&
                leaf.LeafEndTime == DateTime.MinValue &&
                leaf.LeafStartTime == DateTime.MaxValue &&
                leaf.Level == SubGridTree.SubGridTreeLevels &&
                leaf.IsLeafSubGrid() == true,
                "Leaf not initialised as expected after AllocateFullPassStacks");
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_AllocateLatestPassGrid()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            IServerLeafSubGrid leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            leaf.AllocateLeafFullPassStacks();
            leaf.CreateDefaultSegment();
            leaf.AllocateLatestPassGrid(leaf.Directory.SegmentDirectory.First());

            Assert.True(leaf.Directory.SegmentDirectory.First().Segment != null &&
                leaf.Directory.SegmentDirectory.First().Segment.LatestPasses != null, 
                "Segment cell passes not created by AllocateLatestPassGrid");
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_AllocateLeafFullPassStacks()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            IServerLeafSubGrid leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            leaf.AllocateLeafFullPassStacks();

            Assert.NotNull(leaf.Cells);
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_AllocateLeafLatestPassGrid()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            IServerLeafSubGrid leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            leaf.AllocateLeafLatestPassGrid();

            Assert.Equal(leaf.Directory.GlobalLatestCells[0, 0].Time, DateTime.MinValue);
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_CellHasValue()
        {
            // Add a cell pass and check the CellHasValue flags the cell as having a value
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            IServerLeafSubGrid leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            leaf.AllocateLeafFullPassStacks();
            leaf.CreateDefaultSegment();
            leaf.AllocateFullPassStacks(leaf.Directory.SegmentDirectory.First());
            leaf.AllocateLeafLatestPassGrid();

            Assert.True(leaf.Directory.GlobalLatestCells != null && 
                          leaf.Directory.GlobalLatestCells.PassDataExistanceMap != null, 
                          "Pass data existence map is not instantiated");

            Assert.False(leaf.CellHasValue(0, 0), "Cell already has a value");

            CellPass pass = CreateTestCellPass();
            leaf.AddPass(0, 0, pass);

            leaf.ComputeLatestPassInformation(true, StorageProxyFactory.Storage(StorageMutability.Mutable));

            Assert.True(leaf.CellHasValue(0, 0), "Cell does not have value");
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_ComputeLatestPassInformation()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            IServerLeafSubGrid leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            leaf.AllocateLeafFullPassStacks();
            leaf.CreateDefaultSegment();
            leaf.AllocateFullPassStacks(leaf.Directory.SegmentDirectory.First());
            leaf.AllocateLeafLatestPassGrid();

            Assert.True(leaf.Directory.GlobalLatestCells != null &&
                          leaf.Directory.GlobalLatestCells.PassDataExistanceMap != null,
                          "Pass data existence map is not instantiated");

            Assert.False(leaf.CellHasValue(0, 0), "Cell already has a value");

            // Add three passes them compute the latest pass information and ensure it matches with the cell passes
            CellPass pass = CreateTestCellPass();
            leaf.AddPass(0, 0, pass);

            pass.Time.AddMinutes(1);
            leaf.AddPass(0, 0, pass);

            pass.Time.AddMinutes(1);
            leaf.AddPass(0, 0, pass);

            leaf.ComputeLatestPassInformation(true, StorageProxyFactory.Storage(StorageMutability.Mutable));

            Assert.True(leaf.CellHasValue(0, 0), "Cell does not have value");

            CellPass latestPass = leaf.Directory.GlobalLatestCells[0, 0];

            Assert.True(latestPass.Equals(pass), "Latest cell pass does not match pass");
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_ServerSubGridTreeLeaf_LoadSegmentFromStorage()
        {
            Assert.True(false);
        }
    }
}

