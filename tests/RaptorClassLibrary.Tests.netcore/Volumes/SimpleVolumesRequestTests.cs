using System;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Requests;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Responses;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.Rendering.Servers.Client;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Volumes;
using Xunit;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests.Volumes
{
    public class SimpleVolumesRequestTests
    {
        RaptorSimpleVolumesServer Server = RaptorSimpleVolumesServer.NewInstance();

        [Fact]
        public void Test_SimpleVolumesRequest_Creation1()
        {
            SimpleVolumesRequest_ApplicationService request = new SimpleVolumesRequest_ApplicationService();

            Assert.NotNull(request);
        }

        [Fact]
        public void Test_SimpleVolumesRequest_Creation2()
        {
            SimpleVolumesRequest_ClusterCompute request = new SimpleVolumesRequest_ClusterCompute();

            Assert.NotNull(request);
        }

        [Fact]
        public void Test_SimpleVolumesRequest_DefaultFilterToFilter_Execute()
        {
            SimpleVolumesRequest_ApplicationService request = new SimpleVolumesRequest_ApplicationService();
            SimpleVolumesRequestArgument arg = new SimpleVolumesRequestArgument()
            {
                SiteModelID = 6,
                VolumeType = VolumeComputationType.Between2Filters,
                BaseFilter = new CombinedFilter()
                {
                    AttributeFilter = 
                    {
                        ReturnEarliestFilteredCellPass = true,
                    }
                },
                TopFilter = new CombinedFilter(),
                BaseDesignID = long.MinValue,
                TopDesignID = long.MinValue,
                CutTolerance = 0.001,
                FillTolerance = 0.001
            };

            SimpleVolumesResponse response = request.Execute(arg);

            // This request will fail (Ignite not accessible from unit tests) and will return a 
            // response with null values

            Assert.NotNull(response);
            Assert.True(response.Cut.HasValue &&
                        response.Fill.HasValue &&
                        response.TotalCoverageArea.HasValue &&
                        response.CutArea.HasValue &&
                        response.FillArea.HasValue &&
                        !response.BoundingExtentGrid.Equals(BoundingWorldExtent3D.Null()),
                        // No LL converesion available yet && !response.BoundingExtentLLH.Equals(BoundingWorldExtent3D.Null()),
                        "Reponse is null, unexpected");
        }
    }
}
