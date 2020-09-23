using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace CCSS.WorksOS.Reports.Abstractions.Models.Request
{
  /// <summary>
  /// All reports share this request at present.
  /// Only XLSX format is supported in WorksOS
  /// </summary>
  public class ReportRequest
  {
    [JsonIgnore] public ReportType ReportTypeEnum { get; set; }

    /// <summary>
    /// The title of the report.
    /// </summary>
    [JsonProperty(PropertyName = "reportTitle", Required = Required.Always)]
    public string ReportTitle { get; private set; }

    /// <summary>
    /// Details that needs to be used to call respective api (List of Urls to query)
    /// </summary>
    [JsonProperty(PropertyName = "reportRoutes", Required = Required.Always)]
    public List<ReportRoute> ReportRoutes { get; private set; }

    private ReportRequest()
    {
      ReportTypeEnum = ReportType.Unknown;
    }

    public ReportRequest(ReportType reportType, string reportTitle, List<ReportRoute> reportRoutes)
    {
      ReportTypeEnum = reportType;
      ReportTitle = reportTitle;
      ReportRoutes = reportRoutes;
    }

    public void Validate()
    {
      if (ReportTypeEnum == ReportType.Unknown)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(9, "Report type unknown."));

      if (string.IsNullOrEmpty(ReportTitle) || ReportTitle.Length > 800)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(9, "Report title must be between 1 and 800 characters."));

      if (ReportRoutes == null || !ReportRoutes.Any())
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(9, "Report parameters must be provided."));

      var summaryReportMandatoryRoutes = new List<string>
      {
        MandatoryReportRoute.Filter.ToString(),
        MandatoryReportRoute.ImportedFiles.ToString(),
        MandatoryReportRoute.ProjectName.ToString(),
        MandatoryReportRoute.ProjectExtents.ToString(),
        //MandatoryReportRoute.ColorPalette.ToString(), // todoJeannie are these used?
        //MandatoryReportRoute.MachineDesigns.ToString(),
        //MandatoryReportRoute.ProjectSettings.ToString()
      };

      var optionalSummaryReportRoutes = new List<string>
      {
        OptionalSummaryReportRoute.PassCountSummary.ToString(),
        OptionalSummaryReportRoute.PassCountDetail.ToString(),
        OptionalSummaryReportRoute.Volumes.ToString(),
        OptionalSummaryReportRoute.Elevation.ToString(),
        OptionalSummaryReportRoute.MDPSummary.ToString(),
        OptionalSummaryReportRoute.CMVSummary.ToString(),
        OptionalSummaryReportRoute.CMVChange.ToString(),
        OptionalSummaryReportRoute.CMVDetail.ToString(),
        OptionalSummaryReportRoute.TemperatureSummary.ToString(),
        OptionalSummaryReportRoute.TemperatureDetail.ToString(),
        OptionalSummaryReportRoute.Speed.ToString(),
        OptionalSummaryReportRoute.CutFill.ToString()
      };

      if (!summaryReportMandatoryRoutes.TrueForAll(x => { return ReportRoutes.Exists(r => r.ReportRouteType.Equals(x)); }))
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(9, "Missing report parameter for Summary report."));

      // must be at least 1 of the optional routes
      if (ReportTypeEnum == ReportType.Summary)
      {
        if (!ReportRoutes.Any(r => optionalSummaryReportRoutes.Contains(r.ReportRouteType)))
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(9, "At least 1 optional report parameter must be included for Summary report."));

        if (
          (!summaryReportMandatoryRoutes.TrueForAll(x => { return ReportRoutes.Exists(r => r.ReportRouteType.Equals(x)); })) &&
          (!optionalSummaryReportRoutes.TrueForAll(x => { return ReportRoutes.Exists(r => r.ReportRouteType.Equals(x)); }))
        )
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, "Report parameter not supported for Summary report."));
      }
      // must include the StationOffset route
      else if (ReportTypeEnum == ReportType.StationOffset)
      {
        if (!ReportRoutes.Exists(r => r.ReportRouteType.Equals("StationOffset")))
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, "Report parameter StationOffset must be included for StationOffset report."));

        if (
          (!summaryReportMandatoryRoutes.TrueForAll(x => { return ReportRoutes.Exists(r => r.ReportRouteType.Equals(x)); })) &&
          (!ReportRoutes.Exists(r => r.ReportRouteType.Equals("StationOffset")))
        )
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, "Report parameter not supported for StationOffset report."));
      }
      else // Grid
      {
        if (!ReportRoutes.Exists(r => r.ReportRouteType.Equals("Grid")))
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, "Report parameter Grid must be included for Grid report."));

        if (
          (!summaryReportMandatoryRoutes.TrueForAll(x => { return ReportRoutes.Exists(r => r.ReportRouteType.Equals(x)); })) &&
          (!ReportRoutes.Exists(r => r.ReportRouteType.Equals("Grid")))
        )
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, "Report parameter not supported for Grid report."));

      }

      ReportRoutes.ForEach(r => r.Validate());
    }
  }
}
