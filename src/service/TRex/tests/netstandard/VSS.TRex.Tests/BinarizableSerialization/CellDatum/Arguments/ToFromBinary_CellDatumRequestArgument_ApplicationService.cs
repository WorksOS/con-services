using System;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.CellDatum.Arguments
{
  public class ToFromBinary_CellDatumRequestArgument_ApplicationService : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_CellDatumRequestArgument_ApplicationService_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CellDatumRequestArgument_ApplicationService>("Empty CellDatumRequestArgument_ApplicationService not same after round trip serialisation");
    }

    [Fact]
    public void Test_CellDatumRequestArgument_ApplicationService()
    {
      var argument = new CellDatumRequestArgument_ApplicationService
      {
        ProjectID = Guid.NewGuid(),
        Mode = DisplayMode.Height,
        CoordsAreGrid = false,
        Point = new XYZ(1.234, 5.678),
        Filters = new FilterSet(new CombinedFilter(), new CombinedFilter()),
        ReferenceDesign.DesignID = Guid.Empty,
        ReferenceDesign.Offset = 0
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom CellDatumRequestArgument_ApplicationService not same after round trip serialisation");
    }
  }
}
