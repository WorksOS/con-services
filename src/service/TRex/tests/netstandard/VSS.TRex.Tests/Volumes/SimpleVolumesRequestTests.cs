using System;
using FluentAssertions;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Common;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.ComputeFuncs;
using VSS.TRex.Volumes.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Responses;
using Xunit;

namespace VSS.TRex.Tests.Volumes
{
  public class SimpleVolumesRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
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

    private SimpleVolumesRequestArgument SimpleDefaultRequestArg()
    {
      return new SimpleVolumesRequestArgument
      {
        ProjectID = Guid.NewGuid(),
        VolumeType = VolumeComputationType.Between2Filters,
        BaseFilter = new CombinedFilter
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
    }

    private void CheckResponseContainsNullValues(SimpleVolumesResponse response)
    {
      response.Should().NotBeNull();
      response.Cut.Should().BeNull();
      response.Fill.Should().BeNull();
      response.CutArea.Should().BeNull();
      response.FillArea.Should().BeNull();
      response.TotalCoverageArea.Should().BeNull();
      response.BoundingExtentGrid.Should().BeEquivalentTo(BoundingWorldExtent3D.Null());
    }

    [Fact]
    public void Test_SimpleVolumesRequest_ApplicationService_DefaultFilterToFilter_Execute_NoData()
    {
      DITAGFileAndSubGridRequestsWithIgniteFixture.AddApplicationGridRouting<SimpleVolumesRequestComputeFunc_ApplicationService, SimpleVolumesRequestArgument, SimpleVolumesResponse>();
      DITAGFileAndSubGridRequestsWithIgniteFixture.AddClusterComputeGridRouting<SimpleVolumesRequestComputeFunc_ClusterCompute, SimpleVolumesRequestArgument, SimpleVolumesResponse>();

      SimpleVolumesRequest_ApplicationService request = new SimpleVolumesRequest_ApplicationService();
      SimpleVolumesResponse response = request.Execute(SimpleDefaultRequestArg());

      // This is a no data test, so the response will be null
      CheckResponseContainsNullValues(response);
    }

    [Fact]
    public void Test_SimpleVolumesRequest_ClusterCompute_DefaultFilterToFilter_Execute_NoData()
    {
      DITAGFileAndSubGridRequestsWithIgniteFixture.AddClusterComputeGridRouting<SimpleVolumesRequestComputeFunc_ClusterCompute, SimpleVolumesRequestArgument, SimpleVolumesResponse>();

      SimpleVolumesRequest_ClusterCompute request = new SimpleVolumesRequest_ClusterCompute();

      SimpleVolumesResponse response = request.Execute(SimpleDefaultRequestArg());

      // This is a no data test, so the response will be null
      CheckResponseContainsNullValues(response);
    }
  }
}
