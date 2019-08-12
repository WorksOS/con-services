using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.Productivity3D.Productivity3D.Models.Compaction;
using VSS.TRex.Common.Models;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.Reports
{
  public class StationOffsetReportAutoMapperTests 
  {
    [Theory]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, false, true, false, true, false,
      null, null, "66e6bd66-54d8-4651-8907-88b15d81b2d7",
      1.0, 100, 200, new double[3] { -1, 0, 1 })]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      false, true, false, true, false, true,
      null, null, "66e6bd66-54d8-4651-8907-88b15d81b2d7",
      1.0, 100, 200, new double[3] { -1, 0, 1 })]
    public void MapStationOffsetRequestToResult(
      Guid projectUid, FilterResult filter,
      bool reportElevation, bool reportCmv, bool reportMdp, bool reportPassCount, bool reportTemperature, bool reportCutFill,
      Guid? cutFillDesignUid, double? cutfillDesignOffset, Guid alignmentDesignUid,
      double crossSectionInterval, double startStation, double endStation, double[] offsets)
    { 
      var request = CompactionReportStationOffsetTRexRequest.CreateRequest(
        projectUid, filter,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid, cutfillDesignOffset, alignmentDesignUid,
        crossSectionInterval, startStation, endStation, offsets, null, null);

      var result = AutoMapperUtility.Automapper.Map<StationOffsetReportData_ApplicationService>(request);

      Assert.Equal(request.ReportElevation, result.ReportElevation);
      Assert.Equal(request.ReportCutFill, result.ReportCutFill);
      Assert.Equal(request.ReportCmv, result.ReportCmv);
      Assert.Equal(request.ReportMdp, result.ReportMdp);
      Assert.Equal(request.ReportPassCount, result.ReportPassCount);
      Assert.Equal(request.ReportTemperature, result.ReportTemperature);
      Assert.Equal(0, result.NumberOfRows);
      Assert.NotNull(result.Rows);
    }

    [Theory]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, false, true, false, true, false,
      null, null, "66e6bd66-54d8-4651-8907-88b15d81b2d7",
      1.0, 100, 200, new double[3] { -1, 0, 1 }, false)]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      false, true, false, true, false, true,
      null, null, "66e6bd66-54d8-4651-8907-88b15d81b2d7",
      1.0, 100, 200, new double[3] { -1, 0, 1 }, true)]
    public void MapStationOffsetRequestToApplicationArgument(
      Guid projectUid, FilterResult filter,
      bool reportElevation, bool reportCmv, bool reportMdp, bool reportPassCount, bool reportTemperature, bool reportCutFill,
      Guid? cutFillDesignUid, double? cutfillDesignOffset, Guid alignmentDesignUid,
      double crossSectionInterval, double startStation, double endStation, double[] offsets, bool useOverrides)
    {
      var overrides = useOverrides
        ? new OverridingTargets(75, true, 70, 90, 0, false, 80, 125, new TargetPassCountRange(4, 10),
          new TemperatureSettings(120, 70, true), null)
        : null;

      var liftSettings = useOverrides
        ? new LiftSettings(true, false, SummaryType.Compaction,
          SummaryType.WorkInProgress, 0.2f, LiftDetectionType.AutoMapReset, LiftThicknessType.Compacted,
          new LiftThicknessTarget { TargetLiftThickness = 0.75f, AboveToleranceLiftThickness = 0.3f, BelowToleranceLiftThickness = 0.2f },
          true, 0.5f, true, 0.3, 0.7)
        : null;

      var request = CompactionReportStationOffsetTRexRequest.CreateRequest(
        projectUid, filter,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid, cutfillDesignOffset, alignmentDesignUid,
        crossSectionInterval, startStation, endStation, offsets, overrides, liftSettings);

      var result = AutoMapperUtility.Automapper.Map<StationOffsetReportRequestArgument_ApplicationService>(request);

      Assert.Equal(request.ProjectUid, result.ProjectID);
      Assert.Null(result.Filters);
      Assert.Equal(request.ReportElevation, result.ReportElevation);
      Assert.Equal(request.ReportCutFill, result.ReportCutFill);
      Assert.Equal(request.ReportCmv, result.ReportCmv);
      Assert.Equal(request.ReportMdp, result.ReportMdp);
      Assert.Equal(request.ReportPassCount, result.ReportPassCount);
      Assert.Equal(request.ReportTemperature, result.ReportTemperature);
      Assert.Equal(request.CutFillDesignUid ?? Guid.Empty, result.ReferenceDesign.DesignID);
      Assert.Equal(request.CutFillDesignOffset ?? 0, result.ReferenceDesign.Offset);
      Assert.Equal(request.AlignmentDesignUid, result.AlignmentDesignUid);
      Assert.Equal(request.CrossSectionInterval, result.CrossSectionInterval);
      Assert.Equal(request.StartStation, result.StartStation);
      Assert.Equal(request.EndStation, result.EndStation);
      Assert.Equal(request.Offsets.Length, result.Offsets.Length);
      Assert.Equal(request.Offsets[2], result.Offsets[2]);
      //Overrides mapping tested separately in AutoMapperTests
      if (useOverrides)
      {
        Assert.NotNull(result.Overrides);
        Assert.NotNull(result.LiftParams);
      }
      else
      {
        Assert.Null(result.Overrides);
        Assert.Null(result.LiftParams);
      }
    }

    [Theory]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, false, false, false, false, false,
      null, null, "66e6bd66-54d8-4651-8907-88b15d81b2d7",
      1.0, 100, 200, new double[3] {-1, 0, 1})]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, true, true, true, true, false,
      null, null, "66e6bd66-54d8-4651-8907-88b15d81b2d7",
      1.0, 100, 200, new double[3] {-1, 0, 1})]
    public void StationOffsetTRexRequest_Successful(
      Guid projectUid, FilterResult filter,
      bool reportElevation, bool reportCmv, bool reportMdp, bool reportPassCount, bool reportTemperature, bool reportCutFill,
      Guid? cutFillDesignUid, double? cutfillDesignOffset, Guid alignmentDesignUid,
      double crossSectionInterval, double startStation, double endStation, double[] offsets)
    {
      var request = CompactionReportStationOffsetTRexRequest.CreateRequest(
        projectUid, filter,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid, cutfillDesignOffset, alignmentDesignUid,
        crossSectionInterval, startStation, endStation, offsets, null, null);
      request.Validate();
    }

    [Theory]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      false, false, false, false, false, false,
      null, null, "66e6bd66-54d8-4651-8907-88b15d81b2d7",
      1.0, 100, 200, new double[3] {-1, 0, 1},
      "There are no selected fields to be reported on")]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, false, false, false, true, false,
      null, null, null,
      1.0, 100, 200, new double[3] {-1, 0, 1},
      "Alignment file must be specified for station and offset report.")]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, false, false, false, true, false,
      null, null, "66e6bd66-54d8-4651-8907-88b15d81b2d7",
      400, 100, 200, new double[3] {-1, 0, 1},
      "Interval must be >= 0.1m and <= 100m. Actual value: 400")]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, false, false, false, true, false,
      null, null, "66e6bd66-54d8-4651-8907-88b15d81b2d7",
      1.0, 100, 200, new double[0],
      "Offsets must be specified for station and offset report.")]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, false, false, false, true, false,
      null, null, "66e6bd66-54d8-4651-8907-88b15d81b2d7",
      1.0, 300, 200, new double[3] {-1, 0, 1},
      "Invalid station range for station and offset report.")]
    public void StationOffsetTRexRequest_Unsuccessful(
      Guid projectUid, FilterResult filter,
      bool reportElevation, bool reportCmv, bool reportMdp, bool reportPassCount, bool reportTemperature, bool reportCutFill,
      Guid? cutFillDesignUid, double? cutfillDesignOffset, Guid alignmentDesignUid,
      double crossSectionInterval, double startStation, double endStation, double[] offsets,
      string errorMessage)
    {
      var request = CompactionReportStationOffsetTRexRequest.CreateRequest(
        projectUid, filter,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid, cutfillDesignOffset, alignmentDesignUid,
        crossSectionInterval, startStation, endStation, offsets, null, null);
      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.ValidationError, ex.GetResult.Code);
      Assert.Equal(errorMessage, ex.GetResult.Message);
    }
  }
}


