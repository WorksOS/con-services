using System;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.CellDatum.Responses
{
  public class ToFromBinary_CellDatumResponse_ClusterCompute : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_CellDatumResponse_ClusterCompute_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CellDatumResponse_ClusterCompute>("Empty CellDatumResponse_ClusterCompute not same after round trip serialisation");
    }

    [Fact]
    public void Test_CellDatumResponse_ClusterCompute_MinimalContent()
    {
      var response = new CellDatumResponse_ClusterCompute
      {
        ReturnCode = CellDatumReturnCode.UnexpectedError
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Minimal CellDatumResponse_ClusterCompute not same after round trip serialisation");
    }

    [Fact]
    public void Test_CellDatumResponse_ClusterCompute_WithContent()
    {
      var response = new CellDatumResponse_ClusterCompute
      {
        ReturnCode = CellDatumReturnCode.ValueFound,
        Value = 5,
        TimeStampUTC = DateTime.UtcNow.AddHours(-2.5)
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Custom CellDatumResponse_ClusterCompute not same after round trip serialisation");
    }
  }
}
