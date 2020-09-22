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
      var reportRoutes = new List<ReportRoute>();
      var reportRequest = new ReportRequest(Enum.Parse<ReportType>(reportType), reportTitle, reportRoutes);
      var ex = Assert.Throws<ServiceException>(() => reportRequest.Validate());
      ex.Code.Should().Be(HttpStatusCode.BadRequest);
      ex.GetResult.Code.Should().Be(errorCode);
      ex.GetResult.Message.Should().Be(errorMessage);
    }

    [Fact]
    public void ReportRequest_Summary_Success()
    {
      var reportRoutes = LoadMandatoryReportRoutes();
      var reportRequest = new ReportRequest(ReportType.Summary, "3D Summary Report", reportRoutes);
      reportRequest.Validate();
    }

    [Fact]
    public void ReportRequest_Grid_Success()
    {
      var reportRoutes = LoadMandatoryReportRoutes();
      var reportRequest = new ReportRequest(ReportType.Grid, "Grid Report", reportRoutes);
      reportRequest.Validate();
    }

    [Fact]
    public void ReportRequest_SummaryPlus_Success()
    {
      var reportRoutes = LoadMandatoryReportRoutes();
      LoadOptionalSummaryReportRoutes(reportRoutes);
      var reportRequest = new ReportRequest(ReportType.Summary, "3D Summary Report", reportRoutes);
      reportRequest.Validate();
    }

    [Theory]
    [InlineData("Filter", "https://woteva.com/projectSvc/1.4/project/c8ae5c33", "https://woteva.com/3dpSvc/2.0/reporttiles", "GET", 9, "MapUrl not supported for Filter.")]
    [InlineData("ProjectName", "httwoteva.com/projectSvc/1.4/project/c8ae5c33", "", "get",  9, "QueryUrl is not a valid url format for ProjectName.")]
    public void ReportRequest_Summary_ParameterValidationFailure(string reportRouteType, string queryURL, string mapURL, string method, int errorCode, string errorMessage)
    {
      var reportRoutes = LoadMandatoryReportRoutes();

      // replace if exist, otherwise add
      reportRoutes.Remove(reportRoutes.FirstOrDefault(rp => rp.ReportRouteType == reportRouteType));
      reportRoutes.Add(new ReportRoute(reportRouteType, queryURL, mapURL, method));

      var reportRequest = new ReportRequest(ReportType.Summary, "3D Summary Report", reportRoutes);
      var ex = Assert.Throws<ServiceException>(() => reportRequest.Validate());
      ex.Code.Should().Be(HttpStatusCode.BadRequest);
      ex.GetResult.Code.Should().Be(errorCode);
      ex.GetResult.Message.Should().Be(errorMessage);
    }

    [Theory]
    [InlineData("Blah", "https://woteva.com/projectSvc/1.4/project/c8ae5c33", "", 9, "Report parameter not supported.")]
    public void ReportRequest_StationOffset_ParameterValidationFailure(string reportRouteType, string queryURL, string mapURL, int errorCode, string errorMessage)
    {
      var reportRoutes = LoadMandatoryReportRoutes();

      // replace if exist, otherwise add
      reportRoutes.Remove(reportRoutes.FirstOrDefault(rp => rp.ReportRouteType == reportRouteType));
      reportRoutes.Add(new ReportRoute(reportRouteType, queryURL, mapURL));

      var reportRequest = new ReportRequest(ReportType.StationOffset, "Station Offset Report", reportRoutes);
      var ex = Assert.Throws<ServiceException>(() => reportRequest.Validate());
      ex.Code.Should().Be(HttpStatusCode.BadRequest);
      ex.GetResult.Code.Should().Be(errorCode);
      ex.GetResult.Message.Should().Be(errorMessage);
    }

    private List<ReportRoute> LoadMandatoryReportRoutes()
    {
      var reportRoutes = new List<ReportRoute>();

      reportRoutes.Add(new ReportRoute("ProjectName", 
        "https://woteva.com/projectSvc/1.4/project/c8ae5c33"));
      reportRoutes.Add(new ReportRoute("Filter", 
        "https://woteva.com/filterSvc/1.0/filter/c8ae5c333?filterUid=18e71247"));
      reportRoutes.Add(new ReportRoute("MachineDesigns", 
        "https://woteva.com/3dpSvc/2.0/projects/c8ae5c33/machinedesigns"));
      reportRoutes.Add(new ReportRoute("ColorPalette", 
        "https://woteva.com/3dpSvc/2.0/colorpalettes?projectUid=c8ae5c33"));
      reportRoutes.Add(new ReportRoute("ImportedFiles", 
        "https://woteva.com/projectSvc/1.4/importedfiles?projectUid=c8ae5c33"));
      reportRoutes.Add(new ReportRoute("ProjectSettings", 
        "https://woteva.com/projectSvc/1.4/projectsettings/c8ae5c33"));
      reportRoutes.Add(new ReportRoute("ProjectExtents", 
        "https://woteva.com/3dpSvc/2.0/projectstatistics?projectUid=c8ae5c33"));

      return reportRoutes;
    }

    private void LoadOptionalSummaryReportRoutes(List<ReportRoute> reportRoutes)
    {
      reportRoutes.Add(new ReportRoute("PassCountDetail", 
        "https://woteva.com/3dpSvc/2.0/passcounts/details?projectUid=c8ae5c33&filterUid=18e71247&timestamp=1537736444560",
        "https://woteva.com/3dpSvc/2.0/reporttiles/png?projectUid=c8ae5c33&filterUid=18e71247&width=1024&height=800&overlays=AllOverlays&mode=4&mapType=MAP"));
      reportRoutes.Add(new ReportRoute("CMVDetail", 
        "https://woteva.com/3dpSvc/2.0/cmv/details?projectUid=c8ae5c33&filterUid=a824d675&timestamp=1537746842245",
        "https://woteva.com/3dpSvc/2.0/reporttiles/png?projectUid=c8ae5c33&filterUid=a824d675&width=1024&height=800&overlays=AllOverlays&mode=1&mapType=MAP"));
    }
  }
}
