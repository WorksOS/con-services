using System;
using System.IO;
using System.Net;
using System.Text;
using FluentAssertions;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Reports.Gridded;
using VSS.TRex.Reports.Gridded.GridFabric;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.Reports
{
  public class GriddedReportAutoMapperTests 
  {
    [Theory]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, false, true, false, true, false,
      null, null,
      null, GridReportOption.Automatic,
      800000, 400000, 800001, 400001, 2)]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      false, true, false, true, false, true,
      null, null,
      null, GridReportOption.Automatic,
      800000, 400000, 800001, 400001, 2)]
    public void MapGriddedRequestToResult(
      Guid projectUid, FilterResult filter,
      bool reportElevation, bool reportCmv, bool reportMdp, bool reportPassCount, bool reportTemperature, bool reportCutFill,
      Guid? cutFillDesignUid, double? cutfillDesignOffset,
      double? gridInterval, GridReportOption gridReportOption,
      double startNorthing, double startEasting, double endNorthing, double endEasting, double azimuth)
    {
      var request = new CompactionReportGridTRexRequest(
        projectUid, filter,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid, cutfillDesignOffset,
        gridInterval, gridReportOption, startNorthing, startEasting, endNorthing, endEasting, azimuth);

      var result = AutoMapperUtility.Automapper.Map<GriddedReportData>(request);

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
      null, null,
      null, GridReportOption.Automatic,
      800000, 400000, 800001, 400001, 2)]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      false, true, false, true, false, true,
      null, null,
      null, GridReportOption.Automatic,
      800000, 400000, 800001, 400001, 2)]
    public void MapGriddedRequestToArgument(
      Guid projectUid, FilterResult filter,
      bool reportElevation, bool reportCmv, bool reportMdp, bool reportPassCount, bool reportTemperature, bool reportCutFill,
      Guid? cutFillDesignUid, double? cutfillDesignOffset,
      double? gridInterval, GridReportOption gridReportOption,
      double startNorthing, double startEasting, double endNorthing, double endEasting, double azimuth)
    {
      var request = new CompactionReportGridTRexRequest(
        projectUid, filter,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid, cutfillDesignOffset,
        gridInterval, gridReportOption, startNorthing, startEasting, endNorthing, endEasting, azimuth);

      var result = AutoMapperUtility.Automapper.Map<GriddedReportRequestArgument>(request);

      Assert.Equal(request.ProjectUid, result.ProjectID);
      Assert.Null(result.Filters);
      Assert.Equal(request.CutFillDesignUid ?? Guid.Empty, result.ReferenceDesignUID);
      Assert.Equal(request.CutFillDesignOffset ?? 0, result.ReferenceOffset);
      Assert.Equal(request.ReportElevation, result.ReportElevation);
      Assert.Equal(request.ReportCutFill, result.ReportCutFill);
      Assert.Equal(request.ReportCmv, result.ReportCmv);
      Assert.Equal(request.ReportMdp, result.ReportMdp);
      Assert.Equal(request.ReportPassCount, result.ReportPassCount);
      Assert.Equal(request.ReportTemperature, result.ReportTemperature);
    }

    [Theory]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, false, false, false, false, false,
      null, null,
      null, GridReportOption.Automatic,
      800000, 400000, 800001, 400001, 4)]
    public void GriddedTRexRequest_Successful(
      Guid projectUid, FilterResult filter,
      bool reportElevation, bool reportCmv, bool reportMdp, bool reportPassCount, bool reportTemperature, bool reportCutFill,
      Guid? cutFillDesignUid, double? cutfillDesignOffset, 
      double? gridInterval, GridReportOption gridReportOption, 
      double startNorthing, double startEasting, double endNorthing, double endEasting, double azimuth)
    {
      var request = new CompactionReportGridTRexRequest(
        projectUid, filter,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid, cutfillDesignOffset,
        gridInterval, gridReportOption, startNorthing, startEasting, endNorthing, endEasting, azimuth);
      request.Validate();
    }

    [Theory]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null,
      true, false, false, false, false, false,
      null, null,
      null, GridReportOption.Automatic,
      800000, 400000, 800001, 400001, 66,
      "Azimuth must be in the range 0..2*PI radians. Actual value: 66")]
    public void GriddedTRexRequest_Unsuccessful(
      Guid projectUid, FilterResult filter,
      bool reportElevation, bool reportCmv, bool reportMdp, bool reportPassCount, bool reportTemperature, bool reportCutFill,
      Guid? cutFillDesignUid, double? cutfillDesignOffset,
      double? gridInterval, GridReportOption gridReportOption,
      double startNorthing, double startEasting, double endNorthing, double endEasting, double azimuth,
      string errorMessage)
    {
      var request = new CompactionReportGridTRexRequest(
        projectUid, filter,
        reportElevation, reportCmv, reportMdp, reportPassCount, reportTemperature, reportCutFill,
        cutFillDesignUid, cutfillDesignOffset,
        gridInterval, gridReportOption, startNorthing, startEasting, endNorthing, endEasting, azimuth);

      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.ValidationError, ex.GetResult.Code);
      Assert.Equal(errorMessage, ex.GetResult.Message);
    }

    [Fact]
    public void GriddedReportDataResult()
    {
      var projectUid = Guid.NewGuid();
      var cutFillDesignUid = Guid.NewGuid();
      var cutFillDesignOffset = 1.5;
      var request = new CompactionReportGridTRexRequest(
        projectUid, null,
        true, true, true, true, true, true,
        cutFillDesignUid, cutFillDesignOffset,
        null, GridReportOption.Automatic,
        800000, 400000, 800001, 400001, 2);

      var computeResult = new GriddedReportResult()
      {
        ReturnCode = ReportReturnCode.NoError,
        ReportType = ReportType.Gridded,
        GriddedData = AutoMapperUtility.Automapper.Map<GriddedReportData>(request)
      };
      computeResult.GriddedData.NumberOfRows = 1;
      computeResult.GriddedData.Rows.Add(new GriddedReportDataRow()
      {
        Northing = 1.0,
        Easting = 2.0,
        Elevation = 3.0,
        CutFill = 4.0,
        Cmv = 5,
        Mdp = 6,
        PassCount = 7,
        Temperature = 8
      });
      var executorResult = new GriddedReportDataResult(computeResult.Write());

      var retrieved = new GriddedReportResult();
      using (var ms = new MemoryStream(executorResult.GriddedData))
      {
        using (var reader = new BinaryReader(ms, Encoding.UTF8, true))
        {
          retrieved.ReturnCode = (ReportReturnCode) reader.ReadInt32();
          retrieved.ReportType = (ReportType) reader.ReadInt32();

          retrieved.GriddedData.Read(reader);
        }
      }

      retrieved.ReturnCode.Should().Be(computeResult.ReturnCode);
      retrieved.ReportType.Should().Be(computeResult.ReportType);
      retrieved.GriddedData.Should().NotBeNull();
      retrieved.GriddedData.NumberOfRows.Should().Be(computeResult.GriddedData.NumberOfRows);
      retrieved.GriddedData.Rows[0].Northing.Should().Be(computeResult.GriddedData.Rows[0].Northing);
      retrieved.GriddedData.Rows[0].Easting.Should().Be(computeResult.GriddedData.Rows[0].Easting);
      retrieved.GriddedData.Rows[0].Elevation.Should().Be(computeResult.GriddedData.Rows[0].Elevation);
      retrieved.GriddedData.Rows[0].CutFill.Should().Be(computeResult.GriddedData.Rows[0].CutFill + cutFillDesignOffset);
      retrieved.GriddedData.Rows[0].Cmv.Should().Be(computeResult.GriddedData.Rows[0].Cmv);
      retrieved.GriddedData.Rows[0].Mdp.Should().Be(computeResult.GriddedData.Rows[0].Mdp);
      retrieved.GriddedData.Rows[0].PassCount.Should().Be(computeResult.GriddedData.Rows[0].PassCount);
      retrieved.GriddedData.Rows[0].Temperature.Should().Be(computeResult.GriddedData.Rows[0].Temperature);
    }
  }
}


