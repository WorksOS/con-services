using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Requests;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Responses;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Geometry;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests.Volumes
{
    [TestClass]
    public class SimpleVolumesRequestTests
    {
        [TestMethod]
        public void TesT_SimpleVolumesRequest_Creation()
        {
            SimpleVolumesRequest_ClusterCompute request = new SimpleVolumesRequest_ClusterCompute();

            Assert.IsNotNull(request, "Volumes request failed to create");
        }

        [TestMethod]
        public void Test_SimpleVolumesRequest_Execute()
        {
            SimpleVolumesRequest_ClusterCompute request = new SimpleVolumesRequest_ClusterCompute();
            SimpleVolumesRequestArgument arg = new SimpleVolumesRequestArgument();

            SimpleVolumesResponse response = request.Execute(arg);

            // This request will fail (Ignite not accessibel from unit tests) and will return a 
            // response with null values

            Assert.IsNotNull(response, "Null response returned");
            Assert.IsTrue(!response.Cut.HasValue &&
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
