using System;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
        public class SubGridSpatialAffinityKeyTests
    {
        [Fact]
        public void Test_SubGridSpatialAffinityKey_NullConstructor()
        {
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey();
            Assert.True(key.ProjectUID == Guid.Empty && key.SubGridX == 0 && key.SubGridY == 0 && key.SegmentStartDateTicks == 0 && key.SegmentEndDateTicks == 0,
                "Default constructor sub grid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_SubGridOriginConstructor()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(SubGridSpatialAffinityKey.DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER_TICKS, ID, 12345678, 34567890);
            Assert.True(key.Version == 1 && key.ProjectUID == ID && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentStartDateTicks == -1 && key.SegmentEndDateTicks == -1,
                "Sub grid origin constructor sub grid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_SubGridOriginAndSegmentConstructor()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(2, ID, 12345678, 34567890, 123456, 7891012);
            Assert.True(key.Version == 2 && key.ProjectUID == ID && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentStartDateTicks == 123456 && key.SegmentEndDateTicks == 7891012,
                "Sub grid origin constructor sub grid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_CellAddressConstructor()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(SubGridSpatialAffinityKey.DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER_TICKS, ID, new SubGridCellAddress(12345678, 34567890));
            Assert.True(key.Version == 1 && key.ProjectUID == ID && key.SubGridX == 12345678 && key.SubGridY == 34567890  && key.SegmentStartDateTicks == -1 && key.SegmentEndDateTicks == -1,
                "Cell address constructor sub grid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_CellAddressAndSegmentConstructor()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(2, ID, new SubGridCellAddress(12345678, 34567890), 123456, 789012);
            Assert.True(key.Version == 2 && key.ProjectUID == ID && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentStartDateTicks == 123456 && key.SegmentEndDateTicks == 789012,
                "Cell address constructor sub grid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_ToStringSubGrid()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(SubGridSpatialAffinityKey.DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER_TICKS, ID, new SubGridCellAddress(12345678, 34567890));
            Assert.Equal($"{ID}-1-12345678-34567890", key.ToString());
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_ToStringSegment()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(SubGridSpatialAffinityKey.DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER_TICKS, ID, new SubGridCellAddress(12345678, 34567890), 123456, 789012);
            Assert.Equal($"{ID}-1-12345678-34567890-123456-789012", key.ToString());
        }
    }
}
