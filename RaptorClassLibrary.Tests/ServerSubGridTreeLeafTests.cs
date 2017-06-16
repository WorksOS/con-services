using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Cells;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Tests
{
    [TestClass()]
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
                gpsMode = Raptor.Types.GPSMode.Fixed,
                halfPass = false,
                Height = (float)123.0,
                MachineID = 12345,
                MachineSpeed = 234,
                MaterialTemperature = 700,
                MDP = 800,
                passType = Raptor.Types.PassType.Front,
                RadioLatency = 10,
                RMV = 900,
                SiteModelMachineIndex = 42,
                Time = new DateTime(2000, 1, 1, 1, 1, 1)
            };
        }

        [TestMethod()]
        public void Test_ServerSubGridTreeLeaf_Clear()
        {
            // Add a cell pass and check the CellHasValue flags the cell as having a value
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            leaf.AllocateLeafFullPassStacks();
            leaf.CreateDefaultSegment();
            leaf.AllocateFullPassStacks(leaf.Directory.SegmentDirectory.First());
            leaf.AllocateLeafLatestPassGrid();

            CellPass pass = CreateTestCellPass();
            leaf.AddPass(0, 0, pass);
            leaf.ComputeLatestPassInformation(true);

            leaf.Clear();

            Assert.IsFalse(leaf.CellHasValue(0, 0), "Cell has a value");
            //            Assert.IsTrue(leaf.Directory.SegmentDirectory.First().Segment.PassesData.PassData[0,0].PassCount == 0, "cell pass count is not 0");
            Assert.IsTrue(leaf.Directory.SegmentDirectory.First().Segment == null, "segment reference is not null");
        }

        [TestMethod()]
        public void Test_ServerSubGridTreeLeaf_ServerSubGridTreeLeaf()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            Assert.IsTrue(leaf.Cells == null &&
                leaf.Directory != null &&
                leaf.Dirty == false &&
                leaf.LeafEndTime == DateTime.MinValue &&
                leaf.LeafStartTime == DateTime.MaxValue &&
                leaf.Level == SubGridTree.SubGridTreeLevels &&
                leaf.IsLeafSubGrid() == true,
                "Leaf not initialised as expected");
        }

        [TestMethod()]
        public void Test_ServerSubGridTreeLeaf_AddPass()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            leaf.AllocateLeafFullPassStacks();
            leaf.CreateDefaultSegment();
            leaf.AllocateFullPassStacks(leaf.Directory.SegmentDirectory.First());

            Assert.IsTrue(leaf.Cells != null &&
                          leaf.Directory.SegmentDirectory.First().Segment != null &&
                          leaf.Directory.SegmentDirectory.First().Segment.PassesData != null,
                          "Segment passes data not created correctly for AddPass()");

            CellPass pass = CreateTestCellPass();
            leaf.AddPass(0, 0, pass);

            // Check the cell passes in the segment records the cell pass
            Assert.IsTrue(leaf.Directory.SegmentDirectory.First().Segment.PassesData.SegmentPassCount == 1, "Segment pass count is not 1");
            Assert.IsTrue(leaf.Directory.SegmentDirectory.First().Segment.PassesData.PassCount(0, 0) == 1, "Cell pass count is not 1");
            Assert.IsTrue(leaf.Directory.SegmentDirectory.First().Segment.PassesData.PassTime(0, 0, 0) == new DateTime(2000, 1, 1, 1, 1, 1), "Cell pass has incorrect date");

            // Pull the pass a compare it to what was added
            CellPass pass2 = leaf.Directory.SegmentDirectory.First().Segment.PassesData.Pass(0, 0, 0);
            Assert.IsTrue(pass2.Equals(pass), "Pass retrieved is not the same as the pass asses");

            // Check that the start and end time for the leaf was updated when the cell pass was added.
            Assert.IsTrue(leaf.LeafStartTime == leaf.LeafEndTime && leaf.LeafStartTime == new DateTime(2000, 1, 1, 1, 1, 1), "Leaf start and time was not updated");
        }

        [TestMethod()]
        public void Test_ServerSubGridTreeLeaf_CreateDefaultSegment()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            Assert.IsTrue(leaf.Directory.SegmentDirectory.Count == 0, "Newly created leaf has unexpected segments");

            leaf.CreateDefaultSegment();

            Assert.IsTrue(leaf.Directory.SegmentDirectory.Count == 1, "CreateDefaultSegment did not create default segment");

            Assert.IsTrue(leaf.Directory.SegmentDirectory.First().StartTime == DateTime.MinValue &&
                          leaf.Directory.SegmentDirectory.First().EndTime == DateTime.MaxValue,
                          "Default segment does not have history spanning time range");
        }

        [TestMethod()]
        public void Test_ServerSubGridTreeLeaf_AllocateFullPassStacks()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            leaf.AllocateLeafFullPassStacks();
            leaf.CreateDefaultSegment();
            leaf.AllocateFullPassStacks(leaf.Directory.SegmentDirectory.First());

            Assert.IsTrue(leaf.Cells != null &&
                leaf.Directory != null &&
                leaf.Dirty == false &&
                leaf.LeafEndTime == DateTime.MinValue &&
                leaf.LeafStartTime == DateTime.MaxValue &&
                leaf.Level == SubGridTree.SubGridTreeLevels &&
                leaf.IsLeafSubGrid() == true,
                "Leaf not initialised as expected after AllocateFullPassStacks");
        }

        [TestMethod()]
        public void Test_ServerSubGridTreeLeaf_AllocateLatestPassGrid()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            leaf.AllocateLeafFullPassStacks();
            leaf.CreateDefaultSegment();
            leaf.AllocateLatestPassGrid(leaf.Directory.SegmentDirectory.First());

            Assert.IsTrue(leaf.Directory.SegmentDirectory.First().Segment != null &&
                leaf.Directory.SegmentDirectory.First().Segment.LatestPasses != null, 
                "Segment cell passes not created by AllocateLatestPassGrid");
        }

        [TestMethod()]
        public void Test_ServerSubGridTreeLeaf_AllocateLeafFullPassStacks()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            leaf.AllocateLeafFullPassStacks();

            Assert.IsTrue(leaf.Cells != null, "Full pass stacks for leaf not created by AllocateLeafFullPassStacks");
        }

        [TestMethod()]
        public void Test_ServerSubGridTreeLeaf_AllocateLeafLatestPassGrid()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            leaf.AllocateLeafLatestPassGrid();

            Assert.IsTrue(leaf.Directory.GlobalLatestCells.PassData != null, "Latest pass grid for leaf not created by AllocateLeafLatestPassGrid");
        }

        [TestMethod()]
        public void Test_ServerSubGridTreeLeaf_CellHasValue()
        {
            // Add a cell pass and check the CellHasValue flags the cell as having a value
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            leaf.AllocateLeafFullPassStacks();
            leaf.CreateDefaultSegment();
            leaf.AllocateFullPassStacks(leaf.Directory.SegmentDirectory.First());
            leaf.AllocateLeafLatestPassGrid();

            Assert.IsTrue(leaf.Directory.GlobalLatestCells != null && 
                          leaf.Directory.GlobalLatestCells.PassDataExistanceMap != null, 
                          "Pass data existence map is not instantiated");

            Assert.IsFalse(leaf.CellHasValue(0, 0), "Cell already has a value");

            CellPass pass = CreateTestCellPass();
            leaf.AddPass(0, 0, pass);

            leaf.ComputeLatestPassInformation(true);

            Assert.IsTrue(leaf.CellHasValue(0, 0), "Cell does not have value");
        }

        [TestMethod()]
        public void Test_ServerSubGridTreeLeaf_ComputeLatestPassInformation()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
            ServerSubGridTreeLeaf leaf = new ServerSubGridTreeLeaf(tree, null, SubGridTree.SubGridTreeLevels);

            leaf.AllocateLeafFullPassStacks();
            leaf.CreateDefaultSegment();
            leaf.AllocateFullPassStacks(leaf.Directory.SegmentDirectory.First());
            leaf.AllocateLeafLatestPassGrid();

            Assert.IsTrue(leaf.Directory.GlobalLatestCells != null &&
                          leaf.Directory.GlobalLatestCells.PassDataExistanceMap != null,
                          "Pass data existence map is not instantiated");

            Assert.IsFalse(leaf.CellHasValue(0, 0), "Cell already has a value");

            // Add three passes them compute the latest pass information and ensure it matches with the cell passes
            CellPass pass = CreateTestCellPass();
            leaf.AddPass(0, 0, pass);

            pass.Time.AddMinutes(1);
            leaf.AddPass(0, 0, pass);

            pass.Time.AddMinutes(1);
            leaf.AddPass(0, 0, pass);

            leaf.ComputeLatestPassInformation(true);

            Assert.IsTrue(leaf.CellHasValue(0, 0), "Cell does not have value");

            CellPass latestPass = leaf.Directory.GlobalLatestCells.PassData[0, 0];

            Assert.IsTrue(latestPass.Equals(pass), "Latest cell pass does not match pass");
        }

        [TestMethod()]
        public void Test_ServerSubGridTreeLeaf_LoadSegmentFromStorage()
        {
            Assert.Inconclusive();
        }
    }
}

