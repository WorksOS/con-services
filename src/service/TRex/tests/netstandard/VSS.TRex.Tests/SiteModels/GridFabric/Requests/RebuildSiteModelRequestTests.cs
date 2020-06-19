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
    private void AddPrimaryApplicationGridRouting() => IgniteMock.Mutable.AddApplicationGridRouting<RebuildSiteModelRequestComputeFunc, RebuildSiteModelRequestArgument, RebuildSiteModelRequestResponse>();
    private void AddSecondaryApplicationGridRouting() => IgniteMock.Mutable.AddApplicationGridRouting<DeleteSiteModelRequestComputeFunc, DeleteSiteModelRequestArgument, DeleteSiteModelRequestResponse>();

    private void AddApplicationGridRouting()
    {
      AddPrimaryApplicationGridRouting(); // For the rebuild request
      AddSecondaryApplicationGridRouting(); // For the delete request
    }

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
    public void FailWithPreexistingRebuild()
    {
      // Test we fail creafully if the rebuild request cannot access the delete request
      AddPrimaryApplicationGridRouting();

      // TODO...
      Assert.True(false);
    }


    [Fact]
    public void SucceedWithPreexistingRebuildInCompleteState()
    {
      // Test we fail creafully if the rebuild request cannot access the delete request
      AddPrimaryApplicationGridRouting();

      // TODO...
      Assert.True(false);
    }

    [Fact]
    public void FailWithNoDeleteProjectRequest()
    {
      // Test we fail creafully if the rebuild request cannot access the delete request
      AddPrimaryApplicationGridRouting();

      // TODO...
      Assert.True(false);
    }

    [Fact]
    public void NextTest()
    {
      // Test we fail creafully if the rebuild request cannot access the delete request
      AddApplicationGridRouting();

      // TODO...
      Assert.True(false);
    }
  }
}

