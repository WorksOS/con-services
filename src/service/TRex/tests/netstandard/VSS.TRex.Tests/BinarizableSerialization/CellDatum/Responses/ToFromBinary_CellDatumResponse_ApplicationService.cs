using System;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.CellDatum.Responses
{
  public class ToFromBinary_CellDatumResponse_ApplicationService : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_CellDatumResponse_ApplicationService_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CellDatumResponse_ApplicationService>("Empty CellDatumResponse_ApplicationService not same after round trip serialisation");
    }

    [Fact]
    public void Test_CellDatumResponse_ApplicationService_MinimalContent()
    {
      var response = new CellDatumResponse_ApplicationService
      {
        DisplayMode = DisplayMode.PassCount,
        ReturnCode = CellDatumReturnCode.NoValueFound
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Minimal CellDatumResponse_ApplicationService not same after round trip serialisation");
    }

    [Fact]
    public void Test_CellDatumResponse_ApplicationService_WithContent()
    {
      var response = new CellDatumResponse_ApplicationService
      {
        DisplayMode = DisplayMode.PassCount,
        ReturnCode = CellDatumReturnCode.ValueFound,
        Value = 5,
        TimeStampUTC = DateTime.UtcNow.AddHours(-2.5),
        Northing = 123456.789,
        Easting = 98765.4321
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Custom CellDatumResponse_ApplicationService not same after round trip serialisation");
    }
  }
}
