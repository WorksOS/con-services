using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.GridFabric.Affinity;
using VSS.VisionLink.Raptor.SubGridTrees;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
    [TestClass]
    public class SubGridSpatialAffinityKeyTests
    {
        [TestMethod]
        public void Test_SubGridSpatialAffinityKey_NullConstructor()
        {
            SubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey();
            Assert.IsTrue(key.ProjectID == 0 && key.SubGridX == 0 && key.SubGridY == 0 && key.SegmentIdentifier == String.Empty,
                "Default constructor subgrid spatial affinity key produced unexpected result");
        }

        [TestMethod]
        public void Test_SubGridSpatialAffinityKey_SubGridOriginConstructor()
        {
            SubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(1234, 12345678, 34567890);
            Assert.IsTrue(key.ProjectID == 1234 && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentIdentifier == "",
                "Subgrid origin constructor subgrid spatial affinity key produced unexpected result");
        }

        [TestMethod]
        public void Test_SubGridSpatialAffinityKey_SubGridOriginAndSegmentConstructor()
        {
            SubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(1234, 12345678, 34567890, "123-456-890-012.sgs");
            Assert.IsTrue(key.ProjectID == 1234 && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentIdentifier == "123-456-890-012.sgs",
                "Subgrid origin constructor subgrid spatial affinity key produced unexpected result");
        }

        [TestMethod]
        public void Test_SubGridSpatialAffinityKey_CellAddressConstructor()
        {
            SubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(1234, new SubGridCellAddress(12345678, 34567890));
            Assert.IsTrue(key.ProjectID == 1234 && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentIdentifier == "",
                "Cell address constructor subgrid spatial affinity key produced unexpected result");
        }

        [TestMethod]
        public void Test_SubGridSpatialAffinityKey_CellAddressAndSegmentConstructor()
        {
            SubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(1234, new SubGridCellAddress(12345678, 34567890), "123-456-890-012.sgs");
            Assert.IsTrue(key.ProjectID == 1234 && key.SubGridX == 12345678 && key.SubGridY == 34567890 && key.SegmentIdentifier == "123-456-890-012.sgs",
                "Cell address constructor subgrid spatial affinity key produced unexpected result");
        }

        [TestMethod]
        public void Test_SubGridSpatialAffinityKey_ToStringSubgrid()
        {
            SubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(1234, new SubGridCellAddress(12345678, 34567890), String.Empty);
            Assert.AreEqual("1234-12345678-34567890", key.ToString(),
                "ToString() for subgrid affinity key produced unexpected result");
        }

        [TestMethod]
        public void Test_SubGridSpatialAffinityKey_ToStringSegment()
        {
            SubGridSpatialAffinityKey key = new SubGridSpatialAffinityKey(1234, new SubGridCellAddress(12345678, 34567890), "123-456-890-012.sgs");
            Assert.AreEqual("1234-12345678-34567890-123-456-890-012.sgs", key.ToString(),
                "ToString() for segment affinity key produced unexpected result");
        }
    }
}
