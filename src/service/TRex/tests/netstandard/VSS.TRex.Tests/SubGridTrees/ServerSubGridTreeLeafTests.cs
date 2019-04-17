using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Factories;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
        public class ServerSubGridTreeLeafTests : IClassFixture<DILoggingFixture>
  {
        private CellPass CreateTestCellPass()
        {
            return new CellPass
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
                Time = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc)
            };
        }

        private ServerSubGridTreeLeaf CreateADefaultEmptyLeaf()
        {
          // Add a cell pass and check the CellHasValue flags the cell as having a value
          ServerSubGridTree tree = new ServerSubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>(), StorageMutability.Mutable);
          var leaf = tree.CreateNewSubGrid(SubGridTreeConsts.SubGridTreeLevels) as ServerSubGridTreeLeaf;

          leaf.Clear();
          leaf.AllocateLeafFullPassStacks();
          leaf.CreateDefaultSegment();
          leaf.AllocateFullPassStacks(leaf.Directory.SegmentDirectory.First());
          leaf.AllocateLeafLatestPassGrid();

          return leaf;
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_Clear()
        {
            var leaf = CreateADefaultEmptyLeaf();

            CellPass pass = CreateTestCellPass();
            leaf.AddPass(0, 0, pass);
            leaf.ComputeLatestPassInformation(true, StorageProxy.Instance(StorageMutability.Mutable));

            leaf.Clear();

            Assert.False(leaf.CellHasValue(0, 0), "Cell has a value");
            //            Assert.IsTrue(leaf.Directory.SegmentDirectory.First().Segment.PassesData.PassData[0,0].PassCount == 0, "cell pass count is not 0");
            Assert.Null(leaf.Directory.SegmentDirectory.First().Segment);
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_ServerSubGridTreeLeaf()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>(), StorageMutability.Mutable);
            var leaf = tree.CreateNewSubGrid(SubGridTreeConsts.SubGridTreeLevels) as ServerSubGridTreeLeaf;
            leaf.Clear();

            Assert.True(leaf.Cells == null &&
                leaf.Directory != null &&
                leaf.Dirty == false &&
                leaf.LeafEndTime == Consts.MIN_DATETIME_AS_UTC &&
                leaf.LeafStartTime == Consts.MAX_DATETIME_AS_UTC &&
                leaf.Level == SubGridTreeConsts.SubGridTreeLevels &&
                leaf.IsLeafSubGrid(),
                "Leaf not initialized as expected");
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_AddPass()
        {
            var leaf = CreateADefaultEmptyLeaf();

            Assert.True(leaf.Cells != null &&
                          leaf.Directory.SegmentDirectory.First().Segment != null &&
                          leaf.Directory.SegmentDirectory.First().Segment.PassesData != null,
                          "Segment passes data not created correctly for AddPass()");

            var pass = CreateTestCellPass();
            leaf.AddPass(0, 0, pass);

            // Check the cell passes in the segment records the cell pass
            Assert.Equal(1U, leaf.Directory.SegmentDirectory.First().Segment.PassesData.SegmentPassCount);
            Assert.Equal(1U, leaf.Directory.SegmentDirectory.First().Segment.PassesData.PassCount(0, 0));
            Assert.Equal(leaf.Directory.SegmentDirectory.First().Segment.PassesData.PassTime(0, 0, 0), DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc));

            // Pull the pass a compare it to what was added
            CellPass pass2 = leaf.Directory.SegmentDirectory.First().Segment.PassesData.Pass(0, 0, 0);
            Assert.True(pass2.Equals(pass), "Pass retrieved is not the same as the pass asses");

            // Check that the start and end time for the leaf was updated when the cell pass was added.
            Assert.True(leaf.LeafStartTime == leaf.LeafEndTime && leaf.LeafStartTime == DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), "Leaf start and time was not updated");
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_AddPass_FailWithNullTime()
        {
          var leaf = CreateADefaultEmptyLeaf();
          var pass = new CellPass
          {
            Time = Consts.MIN_DATETIME_AS_UTC
          };

          Action act = () => leaf.AddPass(0, 0, pass);
          act.Should().Throw<TRexSubGridTreeException>().WithMessage("Cell passes added to cell pass stacks must have a non-null, UTC, cell pass time");
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_AddPass_FailWithNonUTCTime()
        {
          var leaf = CreateADefaultEmptyLeaf();
          var pass = new CellPass
          {
            Time =  DateTime.Now
          };

          Action act = () => leaf.AddPass(0, 0, pass);
          act.Should().Throw<TRexSubGridTreeException>().WithMessage("Cell passes added to cell pass stacks must have a non-null, UTC, cell pass time");
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_AddPass_FailWithSegmentSelectionFailure()
        {
          var leaf = CreateADefaultEmptyLeaf();

          var mockCells = new Mock<ISubGridCellPassesDataWrapper>();
          mockCells.Setup(x => x.SelectSegment(It.IsAny<DateTime>())).Returns((ISubGridCellPassesDataSegment)null);
          leaf.Cells = mockCells.Object;

          var pass = new CellPass
          {
            Time = DateTime.UtcNow
          };

          Action act = () => leaf.AddPass(0, 0, pass);
          act.Should().Throw<TRexSubGridTreeException>().WithMessage("Cells.SelectSegment failed to return a segment");
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_CreateDefaultSegment()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>(), StorageMutability.Mutable);
            var leaf = tree.CreateNewSubGrid(SubGridTreeConsts.SubGridTreeLevels) as ServerSubGridTreeLeaf;

            Assert.True(0 == leaf.Directory.SegmentDirectory.Count);

            leaf.CreateDefaultSegment();

            Assert.True(1 == leaf.Directory.SegmentDirectory.Count);

            Assert.True(leaf.Directory.SegmentDirectory.First().StartTime == Consts.MIN_DATETIME_AS_UTC &&
                          leaf.Directory.SegmentDirectory.First().EndTime == Consts.MAX_DATETIME_AS_UTC,
                          "Default segment does not have history spanning time range");
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_AllocateFullPassStacks()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>(), StorageMutability.Mutable);
            var leaf = tree.CreateNewSubGrid(SubGridTreeConsts.SubGridTreeLevels) as ServerSubGridTreeLeaf;

            leaf.Clear();
            leaf.AllocateLeafFullPassStacks();
            leaf.CreateDefaultSegment();
            leaf.AllocateFullPassStacks(leaf.Directory.SegmentDirectory.First());

            Assert.True(leaf.Cells != null &&
                leaf.Directory != null &&
                leaf.Dirty == false &&
                leaf.LeafEndTime == Consts.MIN_DATETIME_AS_UTC &&
                leaf.LeafStartTime == Consts.MAX_DATETIME_AS_UTC &&
                leaf.Level == SubGridTreeConsts.SubGridTreeLevels &&
                leaf.IsLeafSubGrid(),
                "Leaf not initialized as expected after AllocateFullPassStacks");
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_AllocateLatestPassGrid()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>(), StorageMutability.Mutable);
            var leaf = tree.CreateNewSubGrid(SubGridTreeConsts.SubGridTreeLevels) as ServerSubGridTreeLeaf;

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
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>(), StorageMutability.Mutable);
            var leaf = tree.CreateNewSubGrid(SubGridTreeConsts.SubGridTreeLevels) as ServerSubGridTreeLeaf;

            leaf.AllocateLeafFullPassStacks();

            Assert.NotNull(leaf.Cells);
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_AllocateLeafLatestPassGrid()
        {
            ServerSubGridTree tree = new ServerSubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>(), StorageMutability.Mutable);
            var leaf = tree.CreateNewSubGrid(SubGridTreeConsts.SubGridTreeLevels) as ServerSubGridTreeLeaf;

            leaf.AllocateLeafLatestPassGrid();

            Assert.Equal(leaf.Directory.GlobalLatestCells[0, 0].Time, Consts.MIN_DATETIME_AS_UTC);
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_CellHasValue()
        {
            // Add a cell pass and check the CellHasValue flags the cell as having a value
            var leaf = CreateADefaultEmptyLeaf();

            Assert.True(leaf.Directory.GlobalLatestCells != null && 
                          leaf.Directory.GlobalLatestCells.PassDataExistenceMap != null, 
                          "Pass data existence map is not instantiated");

            Assert.False(leaf.CellHasValue(0, 0), "Cell already has a value");

            CellPass pass = CreateTestCellPass();
            leaf.AddPass(0, 0, pass);

            leaf.ComputeLatestPassInformation(true, StorageProxy.Instance(StorageMutability.Mutable));

            Assert.True(leaf.CellHasValue(0, 0), "Cell does not have value");
        }

        [Fact()]
        public void Test_ServerSubGridTreeLeaf_ComputeLatestPassInformation()
        {
            var leaf = CreateADefaultEmptyLeaf();

            Assert.True(leaf.Directory.GlobalLatestCells != null &&
                          leaf.Directory.GlobalLatestCells.PassDataExistenceMap != null,
                          "Pass data existence map is not instantiated");

            Assert.False(leaf.CellHasValue(0, 0), "Cell already has a value");

            // Add three passes them compute the latest pass information and ensure it matches with the cell passes
            CellPass pass = CreateTestCellPass();
            leaf.AddPass(0, 0, pass);

            pass.Time = pass.Time.AddMinutes(1);
            leaf.AddPass(0, 0, pass);

            pass.Time = pass.Time.AddMinutes(1);
            leaf.AddPass(0, 0, pass);

            leaf.ComputeLatestPassInformation(true, StorageProxy.Instance(StorageMutability.Mutable));

            Assert.True(leaf.CellHasValue(0, 0), "Cell does not have value");

            CellPass latestPass = leaf.Directory.GlobalLatestCells[0, 0];

            Assert.True(latestPass.Equals(pass), "Latest cell pass does not match pass");
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_ServerSubGridTreeLeaf_LoadSegmentFromStorage()
        {
            Assert.True(false);
        }

        [Fact]
        public void LoadDirectoryFromFile_FailureModes_NullStreamResult()
        {
          var tree = new ServerSubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>(), StorageMutability.Mutable);
          var leaf = tree.CreateNewSubGrid(SubGridTreeConsts.SubGridTreeLevels) as ServerSubGridTreeLeaf;

          var mockStorage = new Mock<IStorageProxy>();

          MemoryStream stream;
          mockStorage.Setup(x => x.ReadSpatialStreamFromPersistentStore(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<string>(),
            FileSystemStreamType.SubGridDirectory, out stream)).Returns(FileSystemErrorStatus.OK);

          leaf.LoadDirectoryFromFile(mockStorage.Object, "filename").Should().BeFalse();
        }

        [Fact]
        public void LoadDirectoryFromFile_FailureModes_StreamDoesNotExist()
        {
          var tree = new ServerSubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>(), StorageMutability.Mutable);
          var leaf = tree.CreateNewSubGrid(SubGridTreeConsts.SubGridTreeLevels) as ServerSubGridTreeLeaf;

          var mockStorage = new Mock<IStorageProxy>();

          MemoryStream stream;
          mockStorage.Setup(x => x.ReadSpatialStreamFromPersistentStore(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<string>(),
            FileSystemStreamType.SubGridDirectory, out stream)).Returns(FileSystemErrorStatus.FileDoesNotExist);

          leaf.LoadDirectoryFromFile(mockStorage.Object, "filename").Should().BeFalse();
        }


        [Fact]
        public void LoadDirectoryFromFile_Failure_WithWarningModes()
        {
          var tree = new ServerSubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>(), StorageMutability.Mutable);
          var leaf = tree.CreateNewSubGrid(SubGridTreeConsts.SubGridTreeLevels) as ServerSubGridTreeLeaf;

          var mockStorage = new Mock<IStorageProxy>();

          MemoryStream stream;
          mockStorage.Setup(x => x.ReadSpatialStreamFromPersistentStore(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<string>(),
            FileSystemStreamType.SubGridDirectory, out stream)).Returns(FileSystemErrorStatus.SpatialStreamIndexGranuleLocationNull);

          leaf.LoadDirectoryFromFile(mockStorage.Object, "filename").Should().BeFalse();

          mockStorage.Setup(x => x.ReadSpatialStreamFromPersistentStore(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<string>(),
            FileSystemStreamType.SubGridDirectory, out stream)).Returns(FileSystemErrorStatus.GranuleDoesNotExist);

          leaf.LoadDirectoryFromFile(mockStorage.Object, "filename").Should().BeFalse();
        }

        [Fact]
        public void LoadDirectoryFromFile_Success()
        {
          var leaf = CreateADefaultEmptyLeaf();

          MemoryStream stream = new MemoryStream();
          leaf.SaveDirectoryToStream(stream);

          var mockStorage = new Mock<IStorageProxy>();
          mockStorage.Setup(x => x.ReadSpatialStreamFromPersistentStore(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<string>(),
            FileSystemStreamType.SubGridDirectory, out stream)).Returns(FileSystemErrorStatus.OK);

          leaf.LoadDirectoryFromFile(mockStorage.Object, "filename").Should().BeTrue();
        }
    }
}

