using FluentAssertions;
using Xunit;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.SurveyedSurfaces.GridFabric.Requests;

namespace VSS.TRex.Tests.SurveyedSurface.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(RemoveSurveyedSurfaceRequest))]
  public class RemoveSurveyedSurfaceRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var req = new RemoveSurveyedSurfaceRequest();
      req.Should().NotBeNull();
    }
  }
}
