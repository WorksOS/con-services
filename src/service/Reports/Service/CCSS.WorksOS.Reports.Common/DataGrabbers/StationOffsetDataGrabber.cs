using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CCSS.WorksOS.Reports.Common.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.WorksOS.Reports.Common.DataGrabbers
{
  public class StationOffsetDataGrabber : GenericDataGrabber, IDataGrabber
    {
      const string KEY_STATION_OFFSET = "StationOffset";

      public StationOffsetDataGrabber(ILogger logger, IServiceExceptionHandler serviceExceptionHandler, IWebRequest gracefulClient,
        GenericComposerRequest composerRequest)
        : base(logger, serviceExceptionHandler, gracefulClient, composerRequest)
      { }

    public DataGrabberResponse GetReportsData()
    {
      var sw = new Stopwatch();
      sw.Start();

      var response = GenerateReportsData();

      _log.LogInformation($"{nameof(StationOffsetDataGrabber)}.{nameof(GetReportsData)} Final StatusCode: {response.DataGrabberStatus} TotalTime: {Math.Round(sw.Elapsed.TotalSeconds, 2)}");
      return response;
    }

    public override DataGrabberResponse GenerateReportsData()
    {
      var response = new SummaryDataGrabberResponse();

      try
      {
        var parsedData = new Dictionary<string, string>();

        Parallel.ForEach(_composerRequest.ReportRequest.ReportRoutes, report =>
        {
          var sw = new Stopwatch();
          sw.Start();

          var reportRequest = new DataGrabberRequest
          {
            CustomHeaders = _composerRequest.CustomHeaders,
            QueryURL = report.QueryURL,
            SvcMethod = new HttpMethod(report.SvcMethod)
          };

          var reportsData = !string.IsNullOrEmpty(report.QueryURL)
            ? GetData(reportRequest).Result : null;
          var strResponse = reportsData?.Content.ReadAsStringAsync().Result;
          parsedData.Add(report.ReportRouteType, strResponse);

          _log.LogInformation($"{nameof(StationOffsetDataGrabber)}.{nameof(GenerateReportsData)} Time Elapsed for ReportColumn {report.ReportRouteType} is {Math.Round(sw.Elapsed.TotalSeconds, 2)}.");
        });

        response.DataGrabberStatus = (int)HttpStatusCode.OK;

        MapResponseProperties(response, parsedData);
      }
      catch (Exception ex)
      {
        _log.LogError(ex, $"{nameof(StationOffsetDataGrabber)}.{nameof(GenerateReportsData)}: ");
        response.Message = "Internal Server Error";
        response.DataGrabberStatus = (int)HttpStatusCode.InternalServerError;
      }

      return response;
    }

    /// <summary>
    /// Map response data into the report model object.
    /// </summary>
    private void MapResponseProperties(DataGrabberResponse response, IReadOnlyDictionary<string, string> parsedData)
    {
      if (parsedData.ContainsKey(KEY_STATION_OFFSET) && !string.IsNullOrEmpty(parsedData[KEY_STATION_OFFSET]))
      {
        response.ReportData = JsonConvert.DeserializeObject<StationOffsetReportDataModel>(parsedData[KEY_STATION_OFFSET]);
      }
      else
      {
        response.ReportData = null;
        return;
      }

      var stationOffsetReportData = (StationOffsetReportDataModel)response.ReportData;
      MapMandatoryResponseProperties(response, parsedData, stationOffsetReportData);
     
      // Map each report to the query string details for filters section.
      // And strip out just the queries necessary to provide report settings info later during generation.
      _composerRequest.ReportRequest.ReportRoutes?.ForEach(r =>
      {
        if (parsedData.ContainsKey(r.ReportRouteType) &&
                  parsedData[r.ReportRouteType] != null &&
                  r.ReportRouteType == KEY_STATION_OFFSET)
        {
          var splitRequest = r.QueryURL.Split('?');
          if (splitRequest.Length > 1)
            stationOffsetReportData.ReportUrlQueryCollection = HttpUtility.ParseQueryString(splitRequest[1], Encoding.UTF8);
        }
      });

      if (parsedData.ContainsKey(KEY_STATION_OFFSET) && !string.IsNullOrEmpty(parsedData[KEY_STATION_OFFSET]))
      {
        var tempDataModel = JsonConvert.DeserializeObject<JObject>(parsedData[KEY_STATION_OFFSET]);
        var obj = tempDataModel["reportData"]["rows"];
        stationOffsetReportData.Rows = obj.ToObject<StationOffsetReportRow[]>();
      }
    }
  }
}
