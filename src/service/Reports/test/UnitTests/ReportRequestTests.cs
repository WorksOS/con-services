using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CCSS.WorksOS.Reports.Abstractions.Models.Request;
using FluentAssertions;
using VSS.Common.Exceptions;
using Xunit;

namespace CCSS.WorksOS.Reports.UnitTests
{
  public class ReportRequestTests
  {
    // todoJeannie setup error codes
    // todoJeannie more tests e.g. StationOffset and Grid

    [Theory]
    [InlineData("Unknown", "3D Summary Report", 9, "Report type unknown.")]
    [InlineData("StationOffset", "", 9, "Report title must be between 1 and 800 characters.")]
    public void ReportRequest_ValidationFailure(string reportType, string reportTitle, int errorCode, string errorMessage)
    {
      var reportParameters = new List<ReportParameter>();
      var reportRequest = new ReportRequest(Enum.Parse<ReportType>(reportType), reportTitle, reportParameters);
      var ex = Assert.Throws<ServiceException>(() => reportRequest.Validate());
      ex.Code.Should().Be(HttpStatusCode.BadRequest);
      ex.GetResult.Code.Should().Be(errorCode);
      ex.GetResult.Message.Should().Be(errorMessage);
    }

    [Fact]
    public void ReportRequest_Summary_Success()
    {
      var reportParameters = LoadMandatoryReportParameters();
      var reportRequest = new ReportRequest(ReportType.Summary, "3D Summary Report", reportParameters);
      reportRequest.Validate();
    }

    [Fact]
    public void ReportRequest_Grid_Success()
    {
      var reportParameters = LoadMandatoryReportParameters();
      var reportRequest = new ReportRequest(ReportType.Grid, "Grid Report", reportParameters);
      reportRequest.Validate();
    }

    [Fact]
    public void ReportRequest_SummaryPlus_Success()
    {
      var reportParameters = LoadMandatoryReportParameters();
      LoadOptionalSummaryReportParameters(reportParameters);
      var reportRequest = new ReportRequest(ReportType.Summary, "3D Summary Report", reportParameters);
      reportRequest.Validate();
    }

    [Theory]
    [InlineData("Filter", "GET", "https://woteva.com/projectSvc/1.4/project/c8ae5c33", "https://woteva.com/3dpSvc/2.0/reporttiles", 9, "MapUrl not supported for Filter.")]
    [InlineData("Filter", "GET", "httwoteva.com/projectSvc/1.4/project/c8ae5c33", "", 9, "QueryUrl is not a valid url format for Filter.")]
    public void ReportRequest_Summary_ParameterValidationFailure(string reportColumn, string svcMethod, string queryURL, string mapURL, int errorCode, string errorMessage)
    {
      var reportParameters = LoadMandatoryReportParameters();

      // replace if exist, otherwise add
      if (!string.IsNullOrEmpty(reportColumn))
      {
        reportParameters.Remove(reportParameters.FirstOrDefault(rp => rp.ReportParameterType == reportColumn));
        reportParameters.Add(new ReportParameter(reportColumn, svcMethod, queryURL, mapURL));
      }

      var reportRequest = new ReportRequest(ReportType.Summary, "3D Summary Report", reportParameters);
      var ex = Assert.Throws<ServiceException>(() => reportRequest.Validate());
      ex.Code.Should().Be(HttpStatusCode.BadRequest);
      ex.GetResult.Code.Should().Be(errorCode);
      ex.GetResult.Message.Should().Be(errorMessage);
    }

    [Theory]
    [InlineData("Blah", "GET", "https://woteva.com/projectSvc/1.4/project/c8ae5c33", "", 9, "Report parameter not supported.")]
    public void ReportRequest_StationOffset_ParameterValidationFailure(string reportColumn, string svcMethod, string queryURL, string mapURL, int errorCode, string errorMessage)
    {
      var reportParameters = LoadMandatoryReportParameters();

      // replace if exist, otherwise add
      if (!string.IsNullOrEmpty(reportColumn))
      {
        reportParameters.Remove(reportParameters.FirstOrDefault(rp => rp.ReportParameterType == reportColumn));
        reportParameters.Add(new ReportParameter(reportColumn, svcMethod, queryURL, mapURL));
      }

      var reportRequest = new ReportRequest(ReportType.StationOffset, "Station Offset Report", reportParameters);
      var ex = Assert.Throws<ServiceException>(() => reportRequest.Validate());
      ex.Code.Should().Be(HttpStatusCode.BadRequest);
      ex.GetResult.Code.Should().Be(errorCode);
      ex.GetResult.Message.Should().Be(errorMessage);
    }

    private List<ReportParameter> LoadMandatoryReportParameters()
    {
      var reportParameters = new List<ReportParameter>();

      reportParameters.Add(new ReportParameter("ProjectName", "GET",
        "https://woteva.com/projectSvc/1.4/project/c8ae5c33"));
      reportParameters.Add(new ReportParameter("Filter", "GET",
        "https://woteva.com/filterSvc/1.0/filter/c8ae5c333?filterUid=18e71247"));
      reportParameters.Add(new ReportParameter("MachineDesigns", "GET",
        "https://woteva.com/3dpSvc/2.0/projects/c8ae5c33/machinedesigns"));
      reportParameters.Add(new ReportParameter("ColorPalette", "GET",
        "https://woteva.com/3dpSvc/2.0/colorpalettes?projectUid=c8ae5c33"));
      reportParameters.Add(new ReportParameter("ImportedFiles", "GET",
        "https://woteva.com/projectSvc/1.4/importedfiles?projectUid=c8ae5c33"));
      reportParameters.Add(new ReportParameter("ProjectSettings", "GET",
        "https://woteva.com/projectSvc/1.4/projectsettings/c8ae5c33"));
      reportParameters.Add(new ReportParameter("ProjectExtents", "GET",
        "https://woteva.com/3dpSvc/2.0/projectstatistics?projectUid=c8ae5c33"));

      return reportParameters;
    }

    private void LoadOptionalSummaryReportParameters(List<ReportParameter> reportParameters)
    {
      reportParameters.Add(new ReportParameter("PassCountDetail", "GET",
        "https://woteva.com/3dpSvc/2.0/passcounts/details?projectUid=c8ae5c33&filterUid=18e71247&timestamp=1537736444560",
        "https://woteva.com/3dpSvc/2.0/reporttiles/png?projectUid=c8ae5c33&filterUid=18e71247&width=1024&height=800&overlays=AllOverlays&mode=4&mapType=MAP"));
      reportParameters.Add(new ReportParameter("CMVDetail", "GET",
        "https://woteva.com/3dpSvc/2.0/cmv/details?projectUid=c8ae5c33&filterUid=a824d675&timestamp=1537746842245",
        "https://woteva.com/3dpSvc/2.0/reporttiles/png?projectUid=c8ae5c33&filterUid=a824d675&width=1024&height=800&overlays=AllOverlays&mode=1&mapType=MAP"));
    }
  }
}
