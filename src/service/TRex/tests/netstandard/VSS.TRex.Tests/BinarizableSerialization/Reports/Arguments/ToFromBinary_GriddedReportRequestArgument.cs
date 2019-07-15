using System;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Reports.Gridded.GridFabric;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Reports.Arguments

{
  public class ToFromBinary_GriddedReportRequestArgument : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_GriddedReportRequestArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<GriddedReportRequestArgument>("Empty GriddedReportRequestArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_GriddedReportRequestArgument_Empty()
    {
      var request = new GriddedReportRequestArgument() { };

      SimpleBinarizableInstanceTester.TestClass(request, "Empty GriddedReportRequestArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_GriddedReportRequestArgument_WithContent()
    {
      var request = new GriddedReportRequestArgument()
      {
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        ReferenceDesign = new DesignOffset(Guid.NewGuid(), 1.5),
        ReportElevation = true,
        ReportCmv = true,
        ReportMdp = true,
        ReportPassCount = true,
        ReportTemperature = true,
        ReportCutFill = false,
        GridInterval = 1.5,
        GridReportOption = GridReportOption.Direction,
        StartNorthing = 808000,
        StartEasting = 400000,
        EndNorthing = 809000,
        EndEasting = 400100,
        Azimuth = 4.6,
        Overrides = new OverrideParameters { OverrideMachineCCV = true, OverridingMachineCCV = 45 }
      };

      SimpleBinarizableInstanceTester.TestClass(request, "Empty GriddedReportRequestArgument not same after round trip serialisation");
    }
  }
}
