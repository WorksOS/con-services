using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Requests;
using Xunit;
using VSS.TRex.Tests.TestFixtures;

namespace VSS.TRex.Tests.Designs.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(RemoveTTMDesignRequest))]
  public class RemoveSurveyedSurfaceRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var req = new RemoveTTMDesignRequest();
      req.Should().NotBeNull();
    }
  }
}
