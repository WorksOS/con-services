using System;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.CellDatum.Arguments
{
  public class ToFromBinary_CellDatumRequestArgument_ClusterCompute : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_CellDatumRequestArgument_ClusterCompute_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CellDatumRequestArgument_ClusterCompute>("Empty CellDatumRequestArgument_ClusterCompute not same after round trip serialisation");
    }

    [Fact]
    public void Test_CellDatumRequestArgument_ClusterCompute()
    {
      var argument = new CellDatumRequestArgument_ClusterCompute
      {
        ProjectID = Guid.NewGuid(),
        Mode = DisplayMode.Height,
        NEECoords = new XYZ(1.234, 5.678),
        OTGCellX = 65125,
        OTGCellY = 28451,
        Filters = new FilterSet(new CombinedFilter(), new CombinedFilter()),
        ReferenceDesign = new DesignOffset(),
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom CellDatumRequestArgument_ClusterCompute not same after round trip serialisation");
    }
  }
}
