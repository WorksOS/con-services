using FluentAssertions;
using Xunit;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Designs.GridFabric.Requests;

namespace VSS.TRex.Tests.Designs.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(RemoveTTMDesignRequest))]
  public class RemoveTTMDesignRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var req = new RemoveTTMDesignRequest();
      req.Should().NotBeNull();
    }
  }
}
