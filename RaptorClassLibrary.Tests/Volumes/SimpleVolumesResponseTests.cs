using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests.Volumes
{
    [TestClass]
    public class SimpleVolumesResponseTests
    {
        [TestMethod]
        public void Test_SimpleVolumesResponseTests_Creation()
        {
            SimpleVolumesResponse response = new SimpleVolumesResponse();

            Assert.IsNotNull(response, "Simple volumes response failed to create");
        }

        [TestMethod]
        public void Test_SimpleVolumesResponseTests_AggregateWith()
        {
            // Allow this to be null, it will receive all the aggregations
            SimpleVolumesResponse response1 = new SimpleVolumesResponse();
            SimpleVolumesResponse response2 = new SimpleVolumesResponse()
            {
                Cut = 10.0,
                Fill = 20.0,
                BoundingExtentGrid = new Geometry.BoundingWorldExtent3D(1.0, 2.0, 3.0, 4.0, 5.0, 6.0),
                CutArea = 30.0,
                FillArea = 40.0,
                TotalCoverageArea = 100.0
            };

            response1.AggregateWith(response2);

            Assert.IsTrue(response1.Cut == 10.0, "Cut value incorrect after aggregatewith");
            Assert.IsTrue(response1.Fill == 20.0, "Fill value incorrect after aggregatewith");
            Assert.IsTrue(response1.BoundingExtentGrid.Equals(response2.BoundingExtentGrid), "BoundingExtentGrid value incorrect after aggregatewith");
            Assert.IsTrue(response1.CutArea == 30.0, "CutArea value incorrect after aggregatewith");
            Assert.IsTrue(response1.FillArea == 40.0, "FillArea value incorrect after aggregatewith");
            Assert.IsTrue(response1.TotalCoverageArea == 100.0, "TotalCoverageArea value incorrect after aggregatewith");
        }
    }
}
