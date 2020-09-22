using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
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
using VSS.Productivity3D.Filter.Abstractions.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace CCSS.WorksOS.Reports.Common.DataGrabbers
{
  public class StationOffsetDataGrabber : GenericDataGrabber, IDataGrabber
    {
      const string keyStationOffset = "StationOffset";

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
            SvcMethod = report.SvcMethod,
            
          };

          var strResponse = !string.IsNullOrEmpty(report.QueryURL)
                      ? GetData(reportRequest)
                      : null;

          parsedData.Add(report.ReportRouteType, strResponse?.Result);

          _log.LogInformation($"{nameof(StationOffsetDataGrabber)}.{nameof(GenerateReportsData)} Time Elapsed for ReportColumn {report.ReportRouteType} is {Math.Round(sw.Elapsed.TotalSeconds, 2)}.");
        });

        response.DataGrabberStatus = (int)HttpStatusCode.OK;

        MapResponseProperties(response, parsedData);
      }
      catch (Exception ex)
      {
        _log.LogError(ex, $"{nameof(StationOffsetDataGrabber)}.{nameof(GenerateReportsData)}: ", ex);
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
      if (parsedData.ContainsKey(keyStationOffset) && parsedData[keyStationOffset] != null)
      {
        response.ReportData = JsonConvert.DeserializeObject<StationOffsetReportDataModel>(parsedData[keyStationOffset]);
      }
      else
      {
        response.ReportData = null;
        return;
      }

      var stationOffsetReportData = (StationOffsetReportDataModel)response.ReportData;

      if (parsedData.ContainsKey("ProjectName") && parsedData["ProjectName"] != null)
      {
        stationOffsetReportData.ProjectName = JsonConvert.DeserializeObject<ProjectV6DescriptorsSingleResult>(parsedData["ProjectName"]);
      }

      // ProjectExtents...
      if (parsedData.ContainsKey("ProjectExtents") && parsedData["ProjectExtents"] != null)
      {
        stationOffsetReportData.ProjectExtents =
          JsonConvert.DeserializeObject<ProjectStatisticsResult>(parsedData["ProjectExtents"]);
      }

      stationOffsetReportData.Filters = new FilterListData { filterDescriptors = new List<FilterDescriptor>() };

      if (parsedData.ContainsKey("Filter") && parsedData["Filter"] != null)
      {
        var filterDescriptor = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(parsedData["Filter"]);
        if (filterDescriptor?.FilterDescriptor?.FilterJson != null)
        {
          var filterDetails = JsonConvert.DeserializeObject<Filter>(filterDescriptor.FilterDescriptor.FilterJson);
          if (filterDetails != null)
          {
            stationOffsetReportData.ReportFilter = filterDetails;
            stationOffsetReportData.Filters.filterDescriptors.Add(filterDescriptor.FilterDescriptor);
          }
        }
      }

      if (parsedData.ContainsKey("ImportedFiles") && parsedData["ImportedFiles"] != null)
      {
        stationOffsetReportData.ImportedFiles = JsonConvert.DeserializeObject<ImportedFileDescriptorListResult>(parsedData["ImportedFiles"]);
      }

      // Map each report to the query string details for filters section.
      // And strip out just the queries necessary to provide report settings info later during generation.
      _composerRequest.ReportRequest.ReportRoutes?.ForEach(r =>
      {
        if (parsedData.ContainsKey(r.ReportRouteType) &&
                  parsedData[r.ReportRouteType] != null &&
                  r.ReportRouteType == keyStationOffset)
        {
          var splitRequest = r.QueryURL.Split('?');
          if (splitRequest.Length > 1)
            stationOffsetReportData.ReportUrlQueryCollection = HttpUtility.ParseQueryString(splitRequest[1], Encoding.UTF8);
        }
      });


      if (!parsedData.ContainsKey(keyStationOffset) || parsedData[keyStationOffset] == null)
      {
        return;
      }

      var tempDataModel = JsonConvert.DeserializeObject<JObject>(parsedData[keyStationOffset]);
      var obj = tempDataModel["reportData"]["rows"];
      stationOffsetReportData.Rows = obj.ToObject<StationOffsetReportRow[]>();
    }
  }
}
