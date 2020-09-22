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
    [JsonProperty(PropertyName = "reportParams", Required = Required.Always)]
    public List<ReportParameter> ReportParameter { get; private set; }

    private ReportRequest()
    {
      ReportTypeEnum = ReportType.Unknown;
    }

    public ReportRequest(ReportType reportType, string reportTitle, List<ReportParameter> reportParameter)
    {
      ReportTypeEnum = reportType;
      ReportTitle = reportTitle;
      ReportParameter = reportParameter;
    }

    public void Validate()
    {
      if (ReportTypeEnum == ReportType.Unknown)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(9, "Report type unknown."));

      if (string.IsNullOrEmpty(ReportTitle) || ReportTitle.Length > 800)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(9, "Report title must be between 1 and 800 characters."));

      if (ReportParameter == null || !ReportParameter.Any())
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(9, "Report parameters must be provided."));

      var summaryReportMandatoryParameters = new List<string>
      {
        MandatoryReportParameter.ColorPalette.ToString(),
        MandatoryReportParameter.Filter.ToString(),
        MandatoryReportParameter.ImportedFiles.ToString(),
        MandatoryReportParameter.MachineDesigns.ToString(),
        MandatoryReportParameter.ProjectName.ToString(),
        MandatoryReportParameter.ProjectExtents.ToString(),
        MandatoryReportParameter.ProjectSettings.ToString()
      };

      var optionalSummaryReportParameters = new List<string>
      {
        OptionalSummaryReportParameter.PassCountSummary.ToString(),
        OptionalSummaryReportParameter.PassCountDetail.ToString(),
        OptionalSummaryReportParameter.Volumes.ToString(),
        OptionalSummaryReportParameter.Elevation.ToString(),
        OptionalSummaryReportParameter.MDPSummary.ToString(),
        OptionalSummaryReportParameter.CMVSummary.ToString(),
        OptionalSummaryReportParameter.CMVChange.ToString(),
        OptionalSummaryReportParameter.CMVDetail.ToString(),
        OptionalSummaryReportParameter.TemperatureSummary.ToString(),
        OptionalSummaryReportParameter.TemperatureDetail.ToString(),
        OptionalSummaryReportParameter.Speed.ToString(),
        OptionalSummaryReportParameter.CutFill.ToString()
      };

      if (!summaryReportMandatoryParameters.TrueForAll(x => { return ReportParameter.Exists(r => r.ReportParameterType.Equals(x)); }))
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(9, "Missing report parameter for Summary report."));

      if (ReportTypeEnum == ReportType.Summary)
      {
        if (
          (!summaryReportMandatoryParameters.TrueForAll(x => { return ReportParameter.Exists(r => r.ReportParameterType.Equals(x)); })) &&
          (!optionalSummaryReportParameters.TrueForAll(x => { return ReportParameter.Exists(r => r.ReportParameterType.Equals(x)); }))
        )
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, "Report parameter not supported for Summary report."));
      }
      else
      {
        if (!ReportParameter.TrueForAll(x => Enum.TryParse(x.ReportParameterType, out MandatoryReportParameter _)))
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, "Report parameter not supported."));
      }

      ReportParameter.ForEach(r => r.Validate());
    }
  }
}
