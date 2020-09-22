using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CCSS.WorksOS.Reports.Common.DataGrabbers;
using CCSS.WorksOS.Reports.Common.Helpers;
using CCSS.WorksOS.Reports.Common.Models;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using Filter = VSS.MasterData.Repositories.DBModels.Filter;
using FilterDescriptor = Microsoft.AspNetCore.Mvc.Filters.FilterDescriptor;

namespace VSS.Reports.Core.DataGrabbers
{
  public class SummaryDataAPIDataGrabber : GenericDataGrabber, IDataGrabber
    {
        #region summaryendpoint keys
        //These are the ReportColumn names
        const string keyProjectSettings = "ProjectSettings";
        const string keyFilter = "Filter";
        const string keyProjectName = "ProjectName";
        const string keyProjectExtents = "ProjectExtents";
        const string keyColorPalette = "ColorPalette";
        const string keyImportedFiles = "ImportedFiles";
        const string keyMachineDesigns = "MachineDesigns";
        const string keyPassCountSummary = "PassCountSummary";
        const string keyPassCountDetail = "PassCountDetail";
        const string keyMDPSummary = "MDPSummary";
        const string keyCMVSummary = "CMVSummary";
        const string keyCMVChange = "CMVChange";
        const string keyCMVDetail = "CMVDetail";
        const string keyTemperature = "Temperature";//Obsolete
        const string keyTemperatureSummary = "TemperatureSummary";
        const string keyTemperatureDetail = "TemperatureDetail";
        const string keyCutFill = "CutFill";
        const string keyElevation = "Elevation";
        const string keySpeed = "Speed";
        const string keyVolumes = "Volumes";
        #endregion

        public SummaryDataAPIDataGrabber(ILogger logger, IServiceExceptionHandler serviceExceptionHandler, IWebRequest gracefulClient, GenericComposerRequest composerRequest)
            : base(logger, serviceExceptionHandler, gracefulClient, composerRequest)
        { }

    public DataGrabberResponse GetReportsData()
    {
      throw new NotImplementedException();
    }

    //public DataGrabberResponse GetReportsData(GenericComposerRequest composerRequest)
    //{
    //    string classMethod = $"{nameof(SummaryDataAPIDataGrabber)}.{nameof(GetReportsData)}";
    //    SummaryDataGrabberResponse consolidatedResponse = new SummaryDataGrabberResponse();

    //    Stopwatch sw = new Stopwatch();
    //    sw.Start();

    //    //--> Creating Grabber request object
    //    var request = CloneRequestObject(composerRequest);

    //    //--> Get the report data for each endpoint
    //    var response = GenerateReportsData(request) as SummaryDataGrabberResponse;
    //    consolidatedResponse = response;
    //    //TODO Refactor to convert at display time not here in the model
    //    if (response?.ReportData != null)
    //    {

    //        var preferencesHelperResponse = PreferencesHelper.Apply3DProductivityPreferences(response.ReportData as SummaryReportDataModel, composerRequest.UserPreference);
    //        if (preferencesHelperResponse.IsSuccess)
    //        {
    //            consolidatedResponse.ReportData = preferencesHelperResponse.SummaryReportData;
    //            LogService.Info(_requestContext, classMethod, $"User Preferences applied successfully, RepInstID: {composerRequest.ExpReportUID}");
    //        }
    //        else
    //        {
    //            LogService.Info(_requestContext, classMethod, $"Error in Preferences Helper: {preferencesHelperResponse.ErrorMessage}, RepInstID: {composerRequest.ExpReportUID}");
    //        }
    //    }

    //    //TODO : Check if this is required ?
    //    //if (response.DataGrabberStatus == (int)HttpStatusCode.OK && response.ReportData != null)
    //    //{
    //    //    consolidatedResponse = (SummaryDataGrabberResponse)response;
    //    //}
    //    //else if (response.DataGrabberStatus != (int)HttpStatusCode.NotFound)
    //    //{
    //    //    consolidatedResponse = (SummaryDataGrabberResponse)response;
    //    //}

    //    LogService.Info(_requestContext, classMethod,
    //      $"DataGrabber Final StatusCode: {consolidatedResponse.DataGrabberStatus} for RepInstID: {composerRequest.ExpReportUID}  IsScheduledReport: {(composerRequest.FileSaveLocation == FileSaveLocation.SCHEDUELEDREPORTS)} TotalTime: {Math.Round(sw.Elapsed.TotalSeconds, 2)}");

    //    return consolidatedResponse;
    //}

    //public override DataGrabberResponse GenerateReportsData(DataGrabberRequest request)
    //{
    //    string classMethod = "SummaryDataAPIDataGrabber.GenerateReportsData";
    //    DataGrabberResponse response = new SummaryDataGrabberResponse();
    //    var mandatoryReportColumns = new List<string>
    //    {
    //        ThreeDMandatoryReportColumn.ColorPalette.ToString(),
    //        ThreeDMandatoryReportColumn.Filter.ToString(),
    //        ThreeDMandatoryReportColumn.ImportedFiles.ToString(),
    //        ThreeDMandatoryReportColumn.MachineDesigns.ToString(),
    //        ThreeDMandatoryReportColumn.ProjectName.ToString(),
    //        ThreeDMandatoryReportColumn.ProjectExtents.ToString(),
    //        ThreeDMandatoryReportColumn.ProjectSettings.ToString()
    //    };

    //    try
    //    {
    //        List<System.Tuple<string, string, byte[], NameValueCollection, string>> parsedData = new List<System.Tuple<string, string, byte[], NameValueCollection, string>>();
    //        KeyValuePair<string, int> errorState = new KeyValuePair<string, int>();
    //        Parallel.ForEach(
    //            request.ReportParameters, (report, loopState) =>
    //            {
    //                Stopwatch sw = new Stopwatch();
    //                sw.Start();

    //                if (!loopState.IsStopped)
    //                {
    //                    ImageAPIResponse imgResponse = new ImageAPIResponse();
    //                    ServiceClientResponseMsg reportsData = null;
    //                    DataGrabberRequest reportRequest = new DataGrabberRequest()
    //                    {
    //                        QueryURL = report.QueryURL,
    //                        SvcMethod = report.SvcMethod,
    //                        AccessToken = request.AccessToken,
    //                        Headers = request.Headers
    //                    };
    //                    string strResponse = !string.IsNullOrEmpty(report.QueryURL) ? GetData(reportRequest, out reportsData) : null;
    //                    if (reportsData != null && reportsData.ResponseStatusCode == (int)HttpStatusCode.OK)
    //                    {
    //                        if (report.MapURL != null)
    //                        {
    //                            reportRequest.QueryURL = string.IsNullOrEmpty(request.UserPreference?.Language) ? report.MapURL : report.MapURL + $"&language={request.UserPreference.Language}";
    //                            imgResponse = GetImage(reportRequest);
    //                            if (imgResponse.DataGrabberResponseCode != (int)HttpStatusCode.OK)
    //                            {
    //                                imgResponse.Image = null;
    //                            }
    //                        }
    //                        parsedData.Add(Tuple.Create(report.ReportColumn, strResponse, imgResponse.Image, GetQueryParameters(report), report.QueryURL));
    //                        response.DataGrabberStatus = (int)HttpStatusCode.OK;
    //                    }
    //                    else if (reportsData.ResponseStatusCode != (int)HttpStatusCode.OK
    //                    && mandatoryReportColumns.Contains(report.ReportColumn))
    //                    {
    //                        errorState = new KeyValuePair<string, int>(report.ReportColumn, reportsData.ResponseStatusCode);
    //                        loopState.Stop();
    //                    }
    //                }
    //                LogService.Info(_requestContext, classMethod, $"Time Elapsed for ReportColumn {report.ReportColumn} is {Math.Round(sw.Elapsed.TotalSeconds, 2)}.");
    //            });

    //        //set overall response status
    //        if (errorState.Key != null && errorState.Value != 200)
    //        {
    //            LogService.Info(_requestContext, "SummaryDataAPIDataGrabber.GenerateReportsData",
    //              $"Error while retieving data for ReportColumn: {errorState.Key}, StatusCode :{errorState.Value}");
    //            response.DataGrabberStatus = errorState.Value;
    //            return response;
    //        }

    //        if (response.DataGrabberStatus == (int) HttpStatusCode.OK)
    //        {
    //          //-- Mapping
    //          var summaryReportData = new SummaryReportDataModel();
    //          response.ReportData = summaryReportData;
    //          summaryReportData.Data = new List<SummaryDataBase>();
    //          summaryReportData.Filters = new FilterListData();
    //          summaryReportData.Filters.FilterDescriptors = new List<FilterDescriptor>();
    //          //Save the requested order - use QueryURL which is unique key
    //          var sortedData = parsedData.SortBy(request.ReportParameters.Select(rp => rp.QueryURL),
    //            pd => pd.Item5).ToList();
    //          for (var i = 0; i < sortedData.Count; i++)
    //          {
    //            SummaryDataBase data = null;

    //            switch (sortedData[i].Item1)
    //            {
    //              case keyPassCountSummary:
    //                data = JsonConvert.DeserializeObject<PassCountSummary>(sortedData[i].Item2);
    //                break;
    //              case keyPassCountDetail:
    //                data = JsonConvert.DeserializeObject<PassCountDetails>(sortedData[i].Item2);
    //                break;
    //              case keyMDPSummary:
    //                data = JsonConvert.DeserializeObject<MDPSummary>(sortedData[i].Item2);
    //                break;
    //              case keyCMVSummary:
    //                data = JsonConvert.DeserializeObject<CMVSummary>(sortedData[i].Item2);
    //                break;
    //              case keyCMVChange:
    //                data = JsonConvert.DeserializeObject<CMVChange>(sortedData[i].Item2);
    //                break;
    //              case keyCMVDetail:
    //                data = new CMVDetails
    //                {
    //                  CmvDetailsData =
    //                    JsonConvert.DeserializeObject<CmvDetailsData>(sortedData[i].Item2)
    //                };
    //                break;
    //              case keyTemperatureSummary:
    //              case keyTemperature:
    //                //temporary - keep old 'temperature summary' so we don't break existing contract until 3dpm UI is updated.
    //                //Also may be present in old scheduled reports.
    //                data = JsonConvert.DeserializeObject<TemperatureSummary>(sortedData[i].Item2);
    //                break;
    //              case keyTemperatureDetail:
    //                data = new TemperatureDetails
    //                {
    //                  TemperatureDetailsData =
    //                    JsonConvert.DeserializeObject<TemperatureDetailsData>(sortedData[i].Item2)
    //                };
    //                break;
    //              case keyCutFill:
    //                data = JsonConvert.DeserializeObject<CutFillDetails>(sortedData[i].Item2);
    //                break;
    //              case keySpeed:
    //                data = JsonConvert.DeserializeObject<SpeedSummary>(sortedData[i].Item2);
    //                break;
    //              case keyVolumes:
    //               data = JsonConvert.DeserializeObject<SummaryVolumes>(sortedData[i].Item2);
    //                var filterUrl = request.ReportParameters.FirstOrDefault(f => f.ReportColumn.Equals(keyFilter))?.QueryURL;
    //                if (!string.IsNullOrEmpty(filterUrl))
    //                  GetVolumeFilters(request, response, filterUrl, sortedData[i].Item4);
    //                break;
    //              case keyElevation:
    //                data = JsonConvert.DeserializeObject<ElevationPalette>(sortedData[i].Item2);
    //                break;
    //              case keyProjectSettings:                    
    //                var customProjectSettings = JsonConvert.DeserializeObject<CustomProjectSettings>(sortedData[i].Item2);
    //                if (customProjectSettings.settings != null)
    //                {
    //                  summaryReportData.ProjectCustomSettings = customProjectSettings.settings;
    //                }                        
    //                break;
    //              case keyFilter:                  
    //                var filterDescriptor = JsonConvert.DeserializeObject<FilterData>(sortedData[i].Item2);
    //                if (filterDescriptor != null && filterDescriptor.FilterDescriptor?.FilterJson != null)
    //                {
    //                  var filterDetails = JsonConvert.DeserializeObject<Filter>(filterDescriptor.FilterDescriptor.FilterJson);
    //                  if (filterDetails != null)
    //                  {
    //                    summaryReportData.ReportFilter = filterDetails;
    //                    summaryReportData.Filters.FilterDescriptors.Add(filterDescriptor.FilterDescriptor);
    //                  }
    //                }
    //                break;
    //              case keyProjectName:                     
    //                  summaryReportData.ProjectName = JsonConvert.DeserializeObject<Project>(sortedData[i].Item2);                     
    //                  break;
    //              case keyProjectExtents:
    //                  summaryReportData.ProjectExtents = JsonConvert.DeserializeObject<ProjectStatisticsResult>(sortedData[i].Item2);
    //              break;
    //              case keyColorPalette:                    
    //                  summaryReportData.ColorPalette = JsonConvert.DeserializeObject<ColorPalettes>(sortedData[i].Item2);                      
    //                break;
    //              case keyImportedFiles:                     
    //                  summaryReportData.ImportedFiles = JsonConvert.DeserializeObject<ImportedFileData>(sortedData[i].Item2);                      
    //                break;
    //              case keyMachineDesigns:                     
    //                  summaryReportData.DesignData = JsonConvert.DeserializeObject<MachineDesignData>(sortedData[i].Item2);                      
    //                break;
    //              }

    //            if (data != null)
    //            {
    //              Enum.TryParse(request.ReportParameters[i].ReportColumn, out ThreeDSummaryReport reportEnum);
    //              data.ReportEnum = reportEnum;
    //              data.MapImage = sortedData[i].Item3;
    //              data.ReportUrlQueryCollection = sortedData[i].Item4;
    //              summaryReportData.Data.Add(data);
    //            }
    //          }
    //        }

    //        LogService.Info(_requestContext, "SummaryDataAPIDataGrabber.GenerateReportsData", "Summary data extracted successfully from response.");
    //    }
    //    catch (Exception ex)
    //    {
    //        LogService.Error(_requestContext, "SummaryDataAPIDataGrabber.GenerateReportsData", ex);
    //        response.Message = "Internal Server Error";
    //        response.DataGrabberStatus = (int)HttpStatusCode.InternalServerError;
    //    }
    //    return response;
    //}

    //private NameValueCollection GetQueryParameters(ReportParameter report)
    //{
    //  NameValueCollection queryCollection = null;
    //  ThreeDSummaryReport reportEnum;
    //  if (Enum.TryParse(report.ReportColumn, out reportEnum))
    //  {
    //    string[] splitRequest = report.QueryURL.Split('?');
    //    if (splitRequest.Count() > 1)
    //    {
    //      queryCollection = HttpUtility.ParseQueryString(splitRequest[1], Encoding.UTF8);
    //      if (queryCollection != null)
    //      {
    //        if (reportEnum == ThreeDSummaryReport.Volumes)
    //        {
    //          string[] splitMapRequest = report.MapURL.Split('?');
    //          if (splitMapRequest.Count() > 0)
    //          {
    //            var queryMapCollection = HttpUtility.ParseQueryString(splitMapRequest[1], Encoding.UTF8);
    //            if (queryMapCollection != null)
    //            {
    //              queryCollection.Add(Constants.VolumeCalculationType, queryMapCollection[Constants.VolumeCalculationType]);
    //            }
    //          }
    //        }
    //      }
    //    }
    //  }

    //  return queryCollection;
    //}
    //private ImageAPIResponse GetImage(DataGrabberRequest request)
    //{
    //    byte[] imgResult = new[] { new byte { } };
    //    ServiceClientResponseMsg imageData = GetDataFromApi(request);
    //    var contentType = imageData?.ResponseContentType;
    //    if (contentType == "image/png")
    //    {
    //        imgResult = imageData.ResponseContent.ReadAsByteArrayAsync().Result;
    //    }
    //    return new ImageAPIResponse { DataGrabberResponseCode = imageData.ResponseStatusCode, Image = imgResult };
    //}

    //private string GetData(DataGrabberRequest request, out ServiceClientResponseMsg reportsData)
    //{
    //    reportsData = GetDataFromApi(request);
    //    var result = reportsData?.ResponseContent?.ReadAsStringAsync()?.Result;
    //    if (!string.IsNullOrEmpty(result))
    //    {
    //        return result;
    //    }
    //    return string.Empty;
    //}

    //private void GetVolumeFilters(DataGrabberRequest request, DataGrabberResponse response, string filterUrl, NameValueCollection queryCollection)
    //{
    //    try
    //    {
    //        ServiceClientResponseMsg filterResponse = null;
    //        string apiResponse;
    //        DataGrabberRequest filterRequest = new DataGrabberRequest
    //        {
    //            AccessToken = request.AccessToken,
    //            Headers = request.Headers,
    //            SvcMethod = HttpMethod.Get.Method
    //        };

    //        string topurl = DataGrabberHelper.AppendOrUpdateQueryParam(filterUrl, Constants.FilterUid, queryCollection.Get(Constants.TopUid));
    //        string baseurl = DataGrabberHelper.AppendOrUpdateQueryParam(filterUrl, Constants.FilterUid, queryCollection.Get(Constants.BaseUid));

    //        if (topurl != null && !topurl.Equals(filterUrl))
    //        {
    //            filterRequest.QueryURL = topurl;
    //            apiResponse = GetData(filterRequest, out filterResponse);
    //            var filterDescriptor = JsonConvert.DeserializeObject<FilterData>(apiResponse);
    //            if (filterDescriptor != null && filterDescriptor.FilterDescriptor?.FilterJson != null)
    //            {
    //                var filterDetails = JsonConvert.DeserializeObject<Filter>(filterDescriptor.FilterDescriptor.FilterJson);
    //                if (filterDetails != null)
    //                {
    //                    ((SummaryReportDataModel)response.ReportData).Filters.FilterDescriptors.Add(filterDescriptor.FilterDescriptor);
    //                }
    //            }
    //        }

    //        if (baseurl != null && !baseurl.Equals(filterUrl))
    //        {
    //            filterRequest.QueryURL = baseurl;
    //            apiResponse = GetData(filterRequest, out filterResponse);
    //            var filterDescriptor = JsonConvert.DeserializeObject<FilterData>(apiResponse);
    //            if (filterDescriptor != null && filterDescriptor.FilterDescriptor != null)
    //            {
    //                var filterDetails = JsonConvert.DeserializeObject<Filter>(filterDescriptor.FilterDescriptor.FilterJson);
    //                if (filterDetails != null)
    //                {
    //                    ((SummaryReportDataModel)response.ReportData).Filters.FilterDescriptors.Add(filterDescriptor.FilterDescriptor);
    //                }
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        LogService.Error(_requestContext, "SummaryDataAPIDataGrabber.GetVolumeFilters", ex);
    //        response.Message = "Internal Server Error";
    //        response.DataGrabberStatus = (int)HttpStatusCode.InternalServerError;
    //    }
    //}
  }
}
