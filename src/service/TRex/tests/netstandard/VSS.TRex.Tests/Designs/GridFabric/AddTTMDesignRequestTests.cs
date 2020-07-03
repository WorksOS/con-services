using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Designs.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(AddTTMDesignRequest))]
  public class AddSurveyedSurfaceRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var req = new AddTTMDesignRequest();
      req.Should().NotBeNull();
    }
  }
}
