using System;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Requests;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Responses;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Geometry;
using Xunit;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests.Volumes
{
        public class SimpleVolumesRequestTests
    {
        [Fact]
        public void TesT_SimpleVolumesRequest_Creation()
        {
            SimpleVolumesRequest_ClusterCompute request = new SimpleVolumesRequest_ClusterCompute();

            Assert.NotNull(request);
        }

        [Fact]
        public void Test_SimpleVolumesRequest_Execute()
        {
            SimpleVolumesRequest_ClusterCompute request = new SimpleVolumesRequest_ClusterCompute();
            SimpleVolumesRequestArgument arg = new SimpleVolumesRequestArgument();

            SimpleVolumesResponse response = request.Execute(arg);

            // This request will fail (Ignite not accessibel from unit tests) and will return a 
            // response with null values

            Assert.NotNull(response);
            Assert.True(!response.Cut.HasValue &&
                          !response.Fill.HasValue &&
                          !response.TotalCoverageArea.HasValue &&
                          !response.CutArea.HasValue &&
                          !response.FillArea.HasValue &&
                          response.BoundingExtentGrid.Equals(BoundingWorldExtent3D.Null()) &&
                          response.BoundingExtentLLH.Equals(BoundingWorldExtent3D.Null()),
                          "Reponse is not null as expected");
        }
    }
}
