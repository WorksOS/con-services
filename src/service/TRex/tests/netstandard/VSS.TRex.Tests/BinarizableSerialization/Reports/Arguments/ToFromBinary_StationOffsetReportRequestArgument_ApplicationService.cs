using System;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Records;
using VSS.TRex.Filters;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Reports.Arguments

{
  public class ToFromBinary_StationOffsetReportRequestArgument_ApplicationService : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_StationOffsetReportRequestArgument_ApplicationService_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<StationOffsetReportRequestArgument_ApplicationService>("Empty StationOffsetReportRequestArgument_ApplicationService not same after round trip serialisation");
    }

    [Fact]
    public void Test_StationOffsetReportRequestArgument_ApplicationService_Empty()
    {
      var request = new StationOffsetReportRequestArgument_ApplicationService() { };

      SimpleBinarizableInstanceTester.TestClass(request, "Empty StationOffsetReportRequestArgument_ApplicationService not same after round trip serialisation");
    }

    [Fact]
    public void Test_StationOffsetReportRequestArgument_ApplicationService_WithContent()
    {
      var request = new StationOffsetReportRequestArgument_ApplicationService()
      {
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        ReportElevation = true,
        ReportCmv = true,
        ReportMdp = true,
        ReportPassCount = true,
        ReportTemperature = true,
        ReportCutFill = false,
        AlignmentDesignUid = Guid.NewGuid(),
        CrossSectionInterval = 100,
        StartStation = 3500,
        EndStation = 5500,
        Offsets = new double[] {3,4,5},
        Overrides = new OverrideParameters { OverrideTargetPassCount = true, OverridingTargetPassCountRange = new PassCountRangeRecord(3,7)}
      };

      SimpleBinarizableInstanceTester.TestClass(request, "Empty StationOffsetReportRequestArgument_ApplicationService not same after round trip serialisation");
    }
  }
}
