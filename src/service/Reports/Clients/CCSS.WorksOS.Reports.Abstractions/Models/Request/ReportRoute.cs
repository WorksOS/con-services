using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace CCSS.WorksOS.Reports.Abstractions.Models.Request
{
  /// <summary>
  /// All reports share this request at present.
  /// Only XLSX format is supported in WorksOS
  /// </summary>
  public class ReportRoute
  {
    /// <summary>
    ///  Parameter type e.g. ProjectName and other mandatory
    ///   or optional sheets for summary reports e.g. CMVDetail
    /// </summary>
    [JsonProperty("reportRouteType", Required = Required.Always)]
    public string ReportRouteType { get; private set; }
    
    /// <summary>
    /// The URL to hit to get the report data. 
    /// </summary>
    [JsonProperty(PropertyName = "queryUrl", Required = Required.Always)]
    public string QueryURL { get; private set; }

    [JsonProperty("mapUrl")]
    public string MapURL { get; private set; }

    /// <summary>
    /// The HttpMethod to be used for accessing Query URL. 
    /// </summary>
    [JsonProperty(PropertyName = "method")]
    public string SvcMethod { get; private set; } 

    [JsonProperty("body")]
    public string /* JRaw todoJeannie is this ever used for 3dp reports? */
      SvcBody { get; private set; }


    private ReportRoute()
    {
    }

    public ReportRoute(string reportRouteType, string queryURL, string mapURL = null, string svcMethod = "GET", string svcBody = null)
    {
      ReportRouteType = reportRouteType;
      SvcMethod = new HttpMethod(svcMethod ?? "GET").ToString();

      if (!string.IsNullOrEmpty(queryURL))
        QueryURL = queryURL.Trim();

      if (!string.IsNullOrEmpty(mapURL))
        MapURL = mapURL.Trim();

      SvcBody = svcBody;
    }

    public void Validate()
    {
      // queryUrl optional or not Required (? todoJeannie)  for CMVDetail
      if (ReportRouteType != OptionalSummaryReportRoute.CMVDetail.ToString() &&
          ReportRouteType != "StationOffset") /* todoJeannie isWellFormedUriString doesn't work? */
      {
        if (string.IsNullOrEmpty(QueryURL) || QueryURL.Length > 2000)
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, $"QueryUrl should be between 1 and 2000 characters for {ReportRouteType}."));

        if (!Uri.IsWellFormedUriString(QueryURL, UriKind.Absolute))
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, $"QueryUrl is not a valid url format for {ReportRouteType}."));
      }

      // mapUrl required for all optional 
      if (Enum.TryParse(ReportRouteType, out OptionalSummaryReportRoute _))
      {
        if (string.IsNullOrEmpty(MapURL) || MapURL.Length > 2000)
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, $"MapUrl should be between 1 and 2000 characters for {ReportRouteType}."));
        if (!Uri.IsWellFormedUriString(MapURL, UriKind.Absolute))
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, $"MapURL is not a valid url format for {ReportRouteType}."));
      }
      else
      {
        if (!string.IsNullOrEmpty(MapURL))
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, $"MapUrl not supported for {ReportRouteType}."));
      }
    }
  }
}
