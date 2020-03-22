using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.ComputeFuncs;
using VSS.TRex.Volumes.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Responses;
using Xunit;

namespace VSS.TRex.Tests.Volumes
{
  [UnitTestCoveredRequest(RequestType = typeof(ProgressiveVolumesRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(ProgressiveVolumesRequest_ClusterCompute))]
  public class ProgressiveVolumesRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<ProgressiveVolumesRequestComputeFunc_ApplicationService, ProgressiveVolumesRequestArgument, ProgressiveVolumesResponse>();

    private void AddClusterComputeGridRouting()
    {
      IgniteMock.AddClusterComputeGridRouting<ProgressiveVolumesRequestComputeFunc_ClusterCompute, ProgressiveVolumesRequestArgument, ProgressiveVolumesResponse>();
      IgniteMock.AddClusterComputeGridRouting<SubGridProgressiveResponseRequestComputeFunc, ISubGridProgressiveResponseRequestComputeFuncArgument, bool>();
    }

    [Fact]
    public void Creation1()
    {
      var request = new ProgressiveVolumesRequest_ApplicationService();

      Assert.NotNull(request);
    }

    [Fact]
    public void Creation2()
    {
      var request = new ProgressiveVolumesRequest_ClusterCompute();

      Assert.NotNull(request);
    }

  }
}
