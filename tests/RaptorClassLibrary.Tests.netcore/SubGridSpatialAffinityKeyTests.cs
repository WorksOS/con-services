using System;
using VSS.VisionLink.Raptor.GridFabric.Affinity;
using VSS.VisionLink.Raptor.SubGridTrees;
using Xunit;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
        public class SubGridSpatialAffinityKeyTests
    {
        [Fact]
        public void Test_SubGridSpatialAffinityKey_NullConstructor()
        {
            SubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey();
            Assert.True(key.ProjectID == 0 && key.SubGridX == 0 && key.SubGridY == 0 && key.SegmentIdentifier == String.Empty,
                "Default constructor subgrid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_SubGridOriginConstructor()
        {
            SubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(1234, 12345678, 34567890);
            Assert.True(key.ProjectID == 1234 && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentIdentifier == "",
                "Subgrid origin constructor subgrid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_SubGridOriginAndSegmentConstructor()
        {
            SubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(1234, 12345678, 34567890, "123-456-890-012.sgs");
            Assert.True(key.ProjectID == 1234 && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentIdentifier == "123-456-890-012.sgs",
                "Subgrid origin constructor subgrid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_CellAddressConstructor()
        {
            SubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(1234, new SubGridCellAddress(12345678, 34567890));
            Assert.True(key.ProjectID == 1234 && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentIdentifier == "",
                "Cell address constructor subgrid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_CellAddressAndSegmentConstructor()
        {
            SubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(1234, new SubGridCellAddress(12345678, 34567890), "123-456-890-012.sgs");
            Assert.True(key.ProjectID == 1234 && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentIdentifier == "123-456-890-012.sgs",
                "Cell address constructor subgrid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_ToStringSubgrid()
        {
            SubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(1234, new SubGridCellAddress(12345678, 34567890), String.Empty);
            Assert.Equal("1234-12345678-34567890", key.ToString());
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_ToStringSegment()
        {
            SubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(1234, new SubGridCellAddress(12345678, 34567890), "123-456-890-012.sgs");
            Assert.Equal("1234-12345678-34567890-123-456-890-012.sgs", key.ToString());
        }
    }
}
