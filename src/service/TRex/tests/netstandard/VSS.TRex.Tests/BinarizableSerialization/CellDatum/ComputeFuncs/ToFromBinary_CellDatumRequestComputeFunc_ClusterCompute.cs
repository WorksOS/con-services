using System;
using FluentAssertions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.ComputeFuncs;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.CellDatum.ComputeFuncs
{
  public class ToFromBinary_CellDatumRequestComputeFunc_ClusterCompute : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_CellDatumRequestComputeFunc_ClusterCompute_Simple()
    {
      var func = new CellDatumRequestComputeFunc_ClusterCompute();
      SimpleBinarizableInstanceTester.TestClass<CellDatumRequestComputeFunc_ClusterCompute>("Empty CellDatumRequestComputeFunc_ClusterCompute not same after round trip serialisation");
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
        ReferenceDesignUID = Guid.Empty
      };

      var func = new CellDatumRequestComputeFunc_ClusterCompute {Argument = argument};

      SimpleBinarizableInstanceTester.TestClass(func, "Custom CellDatumRequestComputeFunc_ClusterCompute not same after round trip serialisation");
    }

  }
}
