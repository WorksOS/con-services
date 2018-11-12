using System;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Volumes;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Responses;
using Xunit;

namespace VSS.TRex.Tests.Volumes
{
    public class SimpleVolumesRequestTests : IClassFixture<DILoggingFixture>
  {
        // [Fact(Skip = "Not running tests requiring Ignite nodes")]
        // SimpleVolumesServer Server = SimpleVolumesServer.NewInstance();

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

        [Fact(Skip = "Not running tests requiring Ignite nodes")]
        public void Test_SimpleVolumesRequest_ApplicationService_DefaultFilterToFilter_Execute()
        {
            SimpleVolumesRequest_ApplicationService request = new SimpleVolumesRequest_ApplicationService();
            SimpleVolumesRequestArgument arg = new SimpleVolumesRequestArgument()
            {
                ProjectID = Guid.NewGuid(),
                VolumeType = VolumeComputationType.Between2Filters,
                BaseFilter = new CombinedFilter()
                {
                    AttributeFilter = 
                    {
                        ReturnEarliestFilteredCellPass = true,
                    }
                },
                TopFilter = new CombinedFilter(),
                BaseDesignID = Guid.Empty,
                TopDesignID = Guid.Empty,
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

        [Fact(Skip = "Not running tests requiring Ignite nodes")]
        public void Test_SimpleVolumesRequest_ClusterCompute_DefaultFilterToFilter_Execute()
        {
            SimpleVolumesRequest_ClusterCompute request = new SimpleVolumesRequest_ClusterCompute();
            SimpleVolumesRequestArgument arg = new SimpleVolumesRequestArgument()
            {
                ProjectID = Guid.NewGuid(), // = 6; This needs to change to refer to an actual project
                VolumeType = VolumeComputationType.Between2Filters,
                BaseFilter = new CombinedFilter()
                {
                    AttributeFilter =
                    {
                        ReturnEarliestFilteredCellPass = true,
                    }
                },
                TopFilter = new CombinedFilter(),
                BaseDesignID = Guid.Empty,
                TopDesignID = Guid.Empty,
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
