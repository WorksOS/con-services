using System;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Responses;
using Xunit;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests.Volumes
{
        public class SimpleVolumesResponseTests
    {
        [Fact]
        public void Test_SimpleVolumesResponseTests_Creation()
        {
            SimpleVolumesResponse response = new SimpleVolumesResponse();

            Assert.NotNull(response);
        }

        [Fact]
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

            Assert.Equal(response1.Cut, 10.0);
            Assert.Equal(response1.Fill, 20.0);
            Assert.True(response1.BoundingExtentGrid.Equals(response2.BoundingExtentGrid), "BoundingExtentGrid value incorrect after aggregatewith");
            Assert.Equal(response1.CutArea, 30.0);
            Assert.Equal(response1.FillArea, 40.0);
            Assert.Equal(response1.TotalCoverageArea, 100.0);
        }
    }
}
