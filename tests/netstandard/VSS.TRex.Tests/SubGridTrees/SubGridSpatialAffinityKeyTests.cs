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
            Assert.True(key.ProjectID == Guid.Empty && key.SubGridX == 0 && key.SubGridY == 0 && string.IsNullOrEmpty(key.SegmentIdentifier),
                "Default constructor subgrid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_SubGridOriginConstructor()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(ID, 12345678, 34567890);
            Assert.True(key.ProjectID == ID && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentIdentifier == "",
                "Subgrid origin constructor subgrid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_SubGridOriginAndSegmentConstructor()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(ID, 12345678, 34567890, "123-456-890-012.sgs");
            Assert.True(key.ProjectID == ID && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentIdentifier == "123-456-890-012.sgs",
                "Subgrid origin constructor subgrid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_CellAddressConstructor()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(ID, new SubGridCellAddress(12345678, 34567890));
            Assert.True(key.ProjectID == ID && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentIdentifier == "",
                "Cell address constructor subgrid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_CellAddressAndSegmentConstructor()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(ID, new SubGridCellAddress(12345678, 34567890), "123-456-890-012.sgs");
            Assert.True(key.ProjectID == ID && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentIdentifier == "123-456-890-012.sgs",
                "Cell address constructor subgrid spatial affinity key produced unexpected result");
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_ToStringSubgrid()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(ID, new SubGridCellAddress(12345678, 34567890), string.Empty);
            Assert.Equal($"{ID}-12345678-34567890", key.ToString());
        }

        [Fact]
        public void Test_SubGridSpatialAffinityKey_ToStringSegment()
        {
            Guid ID = Guid.NewGuid();
            ISubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(ID, new SubGridCellAddress(12345678, 34567890), "123-456-890-012.sgs");
            Assert.Equal($"{ID}-12345678-34567890-123-456-890-012.sgs", key.ToString());
        }
    }
}
