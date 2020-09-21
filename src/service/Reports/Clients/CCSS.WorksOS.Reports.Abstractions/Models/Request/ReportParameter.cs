using System;
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
  public class ReportParameter
  {
    /// <summary>
    /// The URL to hit to get the report data. 
    /// </summary>
    [JsonProperty(PropertyName = "queryUrl", Required = Required.Always)]
    public string QueryURL { get; private set; }

    [JsonProperty("mapUrl")] public string MapURL { get; private set; }

    /// <summary>
    /// The HttpMethod to be used for accessing Query URL. 
    /// </summary>
    [JsonProperty(PropertyName = "method", Required = Required.Always)]
    public string SvcMethod { get; private set; }

    [JsonProperty("body")]
    public string /* JRaw todoJeannie is this ever used for 3dp reports? */
      SvcBody { get; private set; }

    /// <summary>
    ///  Paramter type e.g. ProjectName and other mandatory
    ///   or optional sheets for summary reports e.g. CMVDetail
    /// </summary>
    [JsonProperty("reportParameterType")]
    public string ReportParameterType { get; private set; }


    private ReportParameter()
    {
    }

    public ReportParameter(string reportParameterType, string svcMethod, string queryURL, string mapURL = null, string svcBody = null)
    {
      ReportParameterType = reportParameterType;
      SvcMethod = (string.IsNullOrEmpty(svcMethod)) ? "get" : svcMethod.Trim();

      if (!string.IsNullOrEmpty(queryURL))
        QueryURL = queryURL.Trim();

      if (!string.IsNullOrEmpty(mapURL))
        MapURL = mapURL.Trim();

      SvcBody = svcBody;
    }

    public void Validate()
    {
      // queryUrl optional or not Required (? todoJeannie)  for CMVDetail
      if (ReportParameterType != OptionalSummaryReportParameter.CMVDetail.ToString())
      {
        if (string.IsNullOrEmpty(QueryURL) || QueryURL.Length > 2000)
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, $"QueryUrl should be between 1 and 2000 characters for {ReportParameterType}."));

        if (!Uri.IsWellFormedUriString(QueryURL, UriKind.Absolute))
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, $"QueryUrl is not a valid url format for {ReportParameterType}."));
      }

      // mapUrl required for all optional 
      if (Enum.TryParse(ReportParameterType, out OptionalSummaryReportParameter _))
      {
        if (string.IsNullOrEmpty(MapURL) || MapURL.Length > 2000)
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, $"MapUrl should be between 1 and 2000 characters for {ReportParameterType}."));
        if (!Uri.IsWellFormedUriString(MapURL, UriKind.Absolute))
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, $"MapURL is not a valid url format for {ReportParameterType}."));
      }
      else
      {
        if (!string.IsNullOrEmpty(MapURL))
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, $"MapUrl not supported for {ReportParameterType}."));
      }
    }
  }
}
