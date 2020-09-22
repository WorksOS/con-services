using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CCSS.WorksOS.Reports.Common.DataGrabbers;
using CCSS.WorksOS.Reports.Common.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Filter.Abstractions.Models.ResultHandling;
using Filter = VSS.MasterData.Repositories.DBModels.Filter;
using FilterDescriptor = Microsoft.AspNetCore.Mvc.Filters.FilterDescriptor;

namespace VSS.Reports.Core.DataGrabbers
{
    public class GridDataGrabber : GenericDataGrabber, IDataGrabber
    {
    const string keyGrid = "Grid";

    public GridDataGrabber(ILogger logger, IServiceExceptionHandler serviceExceptionHandler, IWebRequest gracefulClient, 
      GenericComposerRequest composerRequest)
      : base(logger, serviceExceptionHandler, gracefulClient, composerRequest)
    { }

    public DataGrabberResponse GetReportsData()
    {
      throw new NotImplementedException();
    }

    //public DataGrabberResponse GetReportsData(GenericComposerRequest composerRequest)
    //{
    //  string classMethod = $"{this.GetType().Name}.{MethodBase.GetCurrentMethod().Name}"; ;
    //  DataGrabberResponse consolidatedResponse = new DataGrabberResponse();

    //  Stopwatch sw = new Stopwatch();
    //  sw.Start();

    //  //--> Creating Grabber request object
    //  var request = CloneRequestObject(composerRequest);

    //  //--> Get the report data for each endpoint
    //  var response = GenerateReportsData(request);

    //  //-->TODO: Validate on response and create consolidated Response
    //  if (response.DataGrabberStatus == (int)HttpStatusCode.OK && response.ReportData != null)
    //    consolidatedResponse = response;
    //  else if (response.DataGrabberStatus != (int)HttpStatusCode.NotFound)
    //    consolidatedResponse = response;

    //  LogService.Info(requestContext, classMethod, string.Format("DataGrabber Final StatusCode: {0} for RepInstID: {1}  IsScheduledReport: {2} TotalTime: {3}",
    //    consolidatedResponse.DataGrabberStatus,
    //    composerRequest.ExpReportUID,
    //    (composerRequest.FileSaveLocation == Common.Helper.Enums.FileSaveLocation.SCHEDUELEDREPORTS),
    //    Math.Round(sw.Elapsed.TotalSeconds, 2)));

    //  return consolidatedResponse;
    //}

    //public override DataGrabberResponse GenerateReportsData(DataGrabberRequest request)
    //{
    //  GridDataGrabberResponse response = new GridDataGrabberResponse();

    //  try
    //  {
    //    Dictionary<string, string> parsedData = new Dictionary<string, string>();

    //    Parallel.ForEach(
    //    request.ReportParameters, report =>
    //    {

    //      ServiceClientResponseMsg reportsData = null;
    //      DataGrabberRequest reportRequest = new DataGrabberRequest()
    //      {
    //        QueryURL = report.QueryURL,
    //        SvcMethod = report.SvcMethod,
    //        AccessToken = request.AccessToken,
    //        Headers = request.Headers
    //      };
    //      string strResponse = GetData(reportRequest, out reportsData);
    //      parsedData.Add(report.ReportColumn, strResponse);
    //    });

    //    response.DataGrabberStatus = (int)HttpStatusCode.OK;

    //    // Data model and filters...
    //    response.ReportData = new GridReportDataModel
    //    {
    //      Filters = new FilterListData { FilterDescriptors = new List<FilterDescriptor>() }
    //    };

    //    var gridReportData = (GridReportDataModel) response.ReportData;

    //    // ProjectName...
    //    if (parsedData.ContainsKey("ProjectName") && parsedData["ProjectName"] != null)
    //      gridReportData.ProjectName = JsonConvert.DeserializeObject<ProjectV6DescriptorsSingleResult>(parsedData["ProjectName"]);

    //    // ProjectExtents...
    //    if (parsedData.ContainsKey("ProjectExtents") && parsedData["ProjectExtents"] != null)
    //      gridReportData.ProjectExtents = JsonConvert.DeserializeObject<ProjectStatisticsResult>(parsedData["ProjectExtents"]);

    //    // Filter details...
    //    if (parsedData.ContainsKey("Filter") && parsedData["Filter"] != null)
    //    {
    //      var filterDescriptor = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(parsedData["Filter"]);
    //      if (filterDescriptor?.FilterDescriptor?.FilterJson != null)
    //      {
    //        var filterDetails = JsonConvert.DeserializeObject<Filter>(filterDescriptor.FilterDescriptor.FilterJson);
    //        if (filterDetails != null)
    //        {
    //          gridReportData.ReportFilter = filterDetails;
    //          gridReportData.Filters.FilterDescriptors.Add(filterDescriptor.FilterDescriptor);
    //        }
    //      }
    //    }

    //    // ImportedFiles and DesignData...
    //    if (parsedData.ContainsKey("ImportedFiles") && parsedData["ImportedFiles"] != null)
    //      gridReportData.ImportedFiles = JsonConvert.DeserializeObject<ImportedFileDescriptorListResult>(parsedData["ImportedFiles"]);

    //    //Map each report to the query string details for filters section
    //    request.ReportParameters?.ForEach((r =>
    //    {
    //      if (parsedData.ContainsKey(r.ReportColumn) && parsedData[r.ReportColumn] != null && r.ReportColumn == keyGrid
    //      && gridReportData.ReportUrlQueryCollection == null)
    //      {
    //        string[] splitRequest = r.QueryURL.Split('?');
    //        if (splitRequest.Count() > 1)
    //        {
    //          gridReportData.ReportUrlQueryCollection = HttpUtility.ParseQueryString(splitRequest[1], Encoding.UTF8);
    //        }
    //      }
    //    }));

    //    if (parsedData.ContainsKey(keyGrid) && parsedData[keyGrid] != null)
    //    {
    //      var tempDataModel = JsonConvert.DeserializeObject<JObject>(parsedData[keyGrid]);
    //      var obj = tempDataModel["reportData"]["rows"];
    //      gridReportData.Rows = obj.ToObject<GridReportRow[]>();
    //    }
    //  }
    //  catch (Exception ex)
    //  {
    //    LogService.Error(requestContext, "SummaryDataAPIDataGrabber.GenerateReportsData", ex);
    //    response.Message = "Internal Server Error";
    //    response.DataGrabberStatus = (int)HttpStatusCode.InternalServerError;
    //  }
    //  return response;
    //}
  }
}
