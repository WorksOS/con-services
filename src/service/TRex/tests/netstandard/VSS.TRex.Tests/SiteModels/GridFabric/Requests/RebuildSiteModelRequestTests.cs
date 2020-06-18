using FluentAssertions;
using VSS.TRex.SiteModels.GridFabric.ComputeFuncs;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.GridFabric.Requests
{
  [UnitTestCoveredRequest(RequestType = typeof(RebuildSiteModelRequest))]
  public class RebuildSiteModelRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationGridRouting() => IgniteMock.Mutable.AddApplicationGridRouting<RebuildSiteModelRequestComputeFunc, RebuildSiteModelRequestArgument, RebuildSiteModelRequestResponse>();

    public RebuildSiteModelRequestTests()
    {
      // This resets all modified content in the Ignite mocks between tests
      DITAGFileAndSubGridRequestsWithIgniteFixture.ResetDynamicMockedIgniteContent();
    }

    [Fact]
    public void Creation()
    {
      var req = new RebuildSiteModelRequest();
      req.Should().NotBeNull();
    }

    [Fact]
    public void NextTest()
    {
      AddApplicationGridRouting();

      /// yada yada
    }
  }
}

