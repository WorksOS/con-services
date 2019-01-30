using System;
using VSS.TRex.Filters;
using Xunit;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Reports.Gridded.GridFabric;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;

namespace VSS.TRex.Tests.BinarizableSerialization.Reports.Arguments

{
  public class ToFromBinary_GriddedReportRequestArgument : BaseTests, IClassFixture<AnalyticsTestsDIFixture>
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
        ReferenceDesignUID = Guid.NewGuid(),
        ReportElevation = true,
        ReportCMV = true,
        ReportMDP = true,
        ReportPassCount = true,
        ReportTemperature = true,
        ReportCutFill = false,
        GridInterval = 1.5,
        GridReportOption = GridReportOption.Direction,
        StartNorthing = 808000,
        StartEasting = 400000,
        EndNorthing = 809000,
        EndEasting = 400100,
        Azimuth = 4.6
      };

      SimpleBinarizableInstanceTester.TestClass(request, "Empty GriddedReportRequestArgument not same after round trip serialisation");
    }
  }
}
