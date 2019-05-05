using FluentAssertions;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.CellDatum.GridFabric
{
  public class CellDatumResponseTests : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_CellDatumResponse_ApplicationService_Creation()
    {
      var response = new CellDatumResponse_ApplicationService();
      response.Should().NotBeNull();
    }

    [Fact]
    public void Test_CellDatumResponse_ClusterCompute_Creation()
    {
      var response = new CellDatumResponse_ClusterCompute();
      response.Should().NotBeNull();
    }
  }
}
