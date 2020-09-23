using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using VSS.Productivity3D.Filter.Abstractions.Models;

namespace CCSS.WorksOS.Reports.Common.DataGrabbers
{
  public class GridDataGrabber : GenericDataGrabber, IDataGrabber
  {
    const string KEY_GRID = "Grid";

    public GridDataGrabber(ILogger logger, IServiceExceptionHandler serviceExceptionHandler, IWebRequest gracefulClient,
      GenericComposerRequest composerRequest)
      : base(logger, serviceExceptionHandler, gracefulClient, composerRequest)
    {
    }

    public DataGrabberResponse GetReportsData()
    {
      var consolidatedResponse = new DataGrabberResponse();

      var sw = new Stopwatch();
      sw.Start();

      // Get the report data for each endpoint
      var response = GenerateReportsData();

      //-->TODO: Validate on response and create consolidated Response
      if (response.DataGrabberStatus == (int) HttpStatusCode.OK && response.ReportData != null)
        consolidatedResponse = response;
      else if (response.DataGrabberStatus != (int) HttpStatusCode.NotFound)
        consolidatedResponse = response;

      _log.LogInformation($"{nameof(GridDataGrabber)}.{nameof(GetReportsData)} Time Elapsed is {Math.Round(sw.Elapsed.TotalSeconds, 2)}.");
      return consolidatedResponse;
    }

    public override DataGrabberResponse GenerateReportsData()
    {
      var response = new GridDataGrabberResponse();

      try
      {
        var parsedData = new Dictionary<string, string>();

        Parallel.ForEach(_composerRequest.ReportRequest.ReportRoutes, report =>
        {
          var reportRequest = new DataGrabberRequest {CustomHeaders = _composerRequest.CustomHeaders, QueryURL = report.QueryURL, SvcMethod = new HttpMethod(report.SvcMethod)};

          var reportsData = GetData(reportRequest).Result;
          var strResponse = reportsData?.Content.ReadAsStringAsync().Result;
          parsedData.Add(report.ReportRouteType, strResponse);
        });

        response.DataGrabberStatus = (int) HttpStatusCode.OK;
        MapResponseProperties(response, parsedData);
      }
      catch (Exception ex)
      {
        _log.LogError(ex, $"{nameof(GridDataGrabber)}.{nameof(GenerateReportsData)}: ");
        response.Message = "Internal Server Error";
        response.DataGrabberStatus = (int) HttpStatusCode.InternalServerError;
      }

      return response;
    }

    /// <summary>
    /// Map response data into the report model object.
    /// </summary>
    private void MapResponseProperties(DataGrabberResponse response, IReadOnlyDictionary<string, string> parsedData)
    {
      // Data model and filters...
      response.ReportData = new GridReportDataModel {Filters = new FilterListData {filterDescriptors = new List<FilterDescriptor>()}};

      var gridReportData = (GridReportDataModel) response.ReportData;

      MapMandatoryResponseProperties(response, parsedData, gridReportData);

      //Map each report to the query string details for filters section
      _composerRequest.ReportRequest.ReportRoutes?.ForEach((r =>
      {
        if (parsedData.ContainsKey(r.ReportRouteType) && 
            parsedData[r.ReportRouteType] != null && 
            r.ReportRouteType == KEY_GRID
            && gridReportData.ReportUrlQueryCollection == null)
        {
          var splitRequest = r.QueryURL.Split('?');
          if (splitRequest.Count() > 1)
          {
            gridReportData.ReportUrlQueryCollection = HttpUtility.ParseQueryString(splitRequest[1], Encoding.UTF8);
          }
        }
      }));

      if (parsedData.ContainsKey(KEY_GRID) && !string.IsNullOrEmpty(parsedData[KEY_GRID]))
      {
        var tempDataModel = JsonConvert.DeserializeObject<JObject>(parsedData[KEY_GRID]);
        var obj = tempDataModel["reportData"]["rows"];
        gridReportData.Rows = obj.ToObject<GridReportRow[]>();
      }
    }
  }
}
