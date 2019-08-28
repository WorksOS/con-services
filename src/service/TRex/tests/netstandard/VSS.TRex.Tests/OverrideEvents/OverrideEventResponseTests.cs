using FluentAssertions;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.OverrideEvents
{
  public class OverrideEventResponseTests : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_OverrideEventResponse_Creation()
    {
      var response = new OverrideEventResponse();
      response.Should().NotBeNull();
    }

  }
}
