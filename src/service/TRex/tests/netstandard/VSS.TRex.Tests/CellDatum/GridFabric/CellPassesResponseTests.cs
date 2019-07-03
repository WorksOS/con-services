using FluentAssertions;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.CellDatum.GridFabric
{
  public class CellPassesResponseTests : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_CellPassesResponse_ApplicationService_Creation()
    {
      var response = new CellPassesResponse();
      response.Should().NotBeNull();
    }
  }
}
