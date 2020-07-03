using FluentAssertions;
using VSS.TRex.SurveyedSurfaces.GridFabric.Requests;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SurveyedSurface.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(AddSurveyedSurfaceRequest))]
  public class AddSurveyedSurfaceRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var req = new AddSurveyedSurfaceRequest();
      req.Should().NotBeNull();
    }
  }
}
