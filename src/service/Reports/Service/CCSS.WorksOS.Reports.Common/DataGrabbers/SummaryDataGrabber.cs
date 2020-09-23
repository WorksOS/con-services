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
using CCSS.WorksOS.Reports.Abstractions.Models.Request;
using CCSS.WorksOS.Reports.Common.Helpers;
using CCSS.WorksOS.Reports.Common.Models;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Filter.Abstractions.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace CCSS.WorksOS.Reports.Common.DataGrabbers
{
  public class SummaryDataGrabber : GenericDataGrabber, IDataGrabber
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

        public SummaryDataGrabber(ILogger logger, IServiceExceptionHandler serviceExceptionHandler, IWebRequest gracefulClient, GenericComposerRequest composerRequest)
            : base(logger, serviceExceptionHandler, gracefulClient, composerRequest)
        { }

    public DataGrabberResponse GetReportsData()
    {
      var consolidatedResponse = new SummaryDataGrabberResponse();

      Stopwatch sw = new Stopwatch();
      sw.Start();

      // Get the report data for each endpoint
      var response = GenerateReportsData() as SummaryDataGrabberResponse;
      consolidatedResponse = response;

      // todoJeannie
      ////TODO Refactor to convert at display time not here in the model
      //if (response?.ReportData != null)
      //{
      //  var preferencesHelperResponse = PreferencesHelper.Apply3DProductivityPreferences(response.ReportData as SummaryReportDataModel, _composerRequest.UserPreference);
      //  if (preferencesHelperResponse.IsSuccess)
      //  {
      //    consolidatedResponse.ReportData = preferencesHelperResponse.SummaryReportData;
      //    _log.LogInformation($"{nameof(SummaryDataAPIDataGrabber)}.{nameof(GetReportsData)} User Preferences applied successfully");
      //  }
      //  else
      //  {
      //    _log.LogInformation($"{nameof(SummaryDataAPIDataGrabber)}.{nameof(GetReportsData)} Error in Preferences Helper: {preferencesHelperResponse.ErrorMessage}");
      //  }
      //}

      //TODO : Check if this is required ?
      //if (response.DataGrabberStatus == (int)HttpStatusCode.OK && response.ReportData != null)
      //{
      //    consolidatedResponse = (SummaryDataGrabberResponse)response;
      //}
      //else if (response.DataGrabberStatus != (int)HttpStatusCode.NotFound)
      //{
      //    consolidatedResponse = (SummaryDataGrabberResponse)response;
      //}

      _log.LogInformation($"{nameof(SummaryDataGrabber)}.{nameof(GetReportsData)} Time Elapsed is {Math.Round(sw.Elapsed.TotalSeconds, 2)}.");
      return consolidatedResponse;
    }

    public override DataGrabberResponse GenerateReportsData()
    {
      DataGrabberResponse response = new SummaryDataGrabberResponse();
      var mandatoryReportColumns = new List<string>
        {
          MandatoryReportRoute.Filter.ToString(),
          MandatoryReportRoute.ImportedFiles.ToString(),
          MandatoryReportRoute.ProjectName.ToString(),
          MandatoryReportRoute.ProjectExtents.ToString(),
          MandatorySummaryReportRoute.ColorPalette.ToString(),
          MandatorySummaryReportRoute.MachineDesigns.ToString(),
          MandatorySummaryReportRoute.ProjectSettings.ToString()
        };

      try
      {
        var parsedData = new List<System.Tuple<string, string, byte[], NameValueCollection, string>>();
        var errorState = new KeyValuePair<string, int>();

        Parallel.ForEach(_composerRequest.ReportRequest.ReportRoutes, (report, loopState) =>
            {
              var sw = new Stopwatch();
              sw.Start();

              if (!loopState.IsStopped)
              {
                var reportRequest = new DataGrabberRequest
                {
                  CustomHeaders = _composerRequest.CustomHeaders,
                  QueryURL = report.QueryURL,
                  SvcMethod = new HttpMethod(report.SvcMethod)
                };

                var reportsData = !string.IsNullOrEmpty(report.QueryURL) 
                  ? GetData(reportRequest).Result : null;
                
                var imgResponse = new ImageAPIResponse();
                if (reportsData != null && reportsData.StatusCode == HttpStatusCode.OK)
                {
                  if (report.MapURL != null)
                  {
                    reportRequest.QueryURL = string.IsNullOrEmpty(_composerRequest.UserPreference?.Language) ? report.MapURL : report.MapURL + $"&language={_composerRequest.UserPreference.Language}";
                    imgResponse = GetImage(reportRequest);
                    if (imgResponse.DataGrabberResponseCode != (int)HttpStatusCode.OK)
                    {
                      imgResponse.Image = null;
                    }
                  }
                  
                  var strResponse = reportsData.Content.ReadAsStringAsync().Result;
                  parsedData.Add(Tuple.Create(report.ReportRouteType, strResponse, imgResponse.Image, GetQueryParameters(report), report.QueryURL));
                  response.DataGrabberStatus = (int)HttpStatusCode.OK;
                }
                else if (reportsData.StatusCode != HttpStatusCode.OK
                    && mandatoryReportColumns.Contains(report.ReportRouteType))
                {
                  errorState = new KeyValuePair<string, int>(report.ReportRouteType, (int) reportsData.StatusCode);
                  loopState.Stop();
                }
              }
              _log.LogInformation($"{nameof(SummaryDataGrabber)}.{nameof(GenerateReportsData)} Time Elapsed is {Math.Round(sw.Elapsed.TotalSeconds, 2)}.");
            });

        //set overall response status
        if (errorState.Key != null && errorState.Value != 200)
        {
          _log.LogInformation($"{nameof(SummaryDataGrabber)}.{nameof(GenerateReportsData)} Error while retrieving data for ReportColumn: {errorState.Key}, StatusCode :{errorState.Value}");
          response.DataGrabberStatus = errorState.Value;
          return response;
        }

        if (response.DataGrabberStatus == (int)HttpStatusCode.OK)
        {
          //-- Mapping
          var summaryReportData = new SummaryReportDataModel();
          response.ReportData = summaryReportData;
          summaryReportData.Data = new List<SummaryDataBase>();
          summaryReportData.Filters = new FilterDescriptorListResult();
          // todoJeannie summaryReportData.Filters.FilterDescriptors = new List<VSS.Productivity3D.Filter.Abstractions.Models.FilterDescriptor>();
          //Save the requested order - use QueryURL which is unique key
          var sortedData = parsedData.SortBy(_composerRequest.ReportRequest.ReportRoutes.Select(rp => rp.QueryURL),
            pd => pd.Item5).ToList();
          for (var i = 0; i < sortedData.Count; i++)
          {
            SummaryDataBase data = null;

            switch (sortedData[i].Item1)
            {
              case keyPassCountSummary:
                // todoJeannie could I get these models from 3dp?
                data = JsonConvert.DeserializeObject<PassCountSummary>(sortedData[i].Item2);
                break;
              case keyPassCountDetail:
                data = JsonConvert.DeserializeObject<PassCountDetails>(sortedData[i].Item2);
                break;
              case keyMDPSummary:
                data = JsonConvert.DeserializeObject<MDPSummary>(sortedData[i].Item2);
                break;
              case keyCMVSummary:
                data = JsonConvert.DeserializeObject<CMVSummary>(sortedData[i].Item2);
                break;
              case keyCMVChange:
                data = JsonConvert.DeserializeObject<CMVChange>(sortedData[i].Item2);
                break;
              case keyCMVDetail:
                data = new CMVDetails
                {
                  CmvDetailsData =
                    JsonConvert.DeserializeObject<CmvDetailsData>(sortedData[i].Item2)
                };
                break;
              case keyTemperatureSummary:
              case keyTemperature:
                //temporary - keep old 'temperature summary' so we don't break existing contract until 3dpm UI is updated.
                //Also may be present in old scheduled reports.
                data = JsonConvert.DeserializeObject<TemperatureSummary>(sortedData[i].Item2);
                break;
              case keyTemperatureDetail:
                data = new TemperatureDetails
                {
                  TemperatureDetailsData =
                    JsonConvert.DeserializeObject<TemperatureDetailsData>(sortedData[i].Item2)
                };
                break;
              case keyCutFill:
                data = JsonConvert.DeserializeObject<CutFillDetails>(sortedData[i].Item2);
                break;
              case keySpeed:
                data = JsonConvert.DeserializeObject<SpeedSummary>(sortedData[i].Item2);
                break;
              case keyVolumes:
                data = JsonConvert.DeserializeObject<SummaryVolumes>(sortedData[i].Item2);
                var filterUrl = _composerRequest.ReportRequest.ReportRoutes.FirstOrDefault(f => f.ReportRouteType.Equals(keyFilter))?.QueryURL;
                if (!string.IsNullOrEmpty(filterUrl))
                  GetVolumeFilters(response, filterUrl, sortedData[i].Item4);
                break;
              case keyElevation:
                data = JsonConvert.DeserializeObject<ElevationPalette>(sortedData[i].Item2);
                break;
              case keyProjectSettings:
                var customProjectSettings = JsonConvert.DeserializeObject<CustomProjectSettings>(sortedData[i].Item2);
                if (customProjectSettings.settings != null)
                {
                  summaryReportData.ProjectCustomSettings = customProjectSettings.settings;
                }
                break;
              case keyFilter:
                var filterDescriptor = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(sortedData[i].Item2);
                if (filterDescriptor != null && filterDescriptor.FilterDescriptor?.FilterJson != null)
                {
                  var filterDetails = JsonConvert.DeserializeObject<Filter>(filterDescriptor.FilterDescriptor.FilterJson);
                  if (filterDetails != null)
                  {
                    summaryReportData.ReportFilter = filterDetails;
                    summaryReportData.Filters.FilterDescriptors.Add(filterDescriptor.FilterDescriptor);
                  }
                }
                break;
              case keyProjectName:
                summaryReportData.ProjectName = JsonConvert.DeserializeObject<ProjectV6DescriptorsSingleResult>(sortedData[i].Item2);
                break;
              case keyProjectExtents:
                summaryReportData.ProjectExtents = JsonConvert.DeserializeObject<ProjectStatisticsResult>(sortedData[i].Item2);
                break;
              case keyColorPalette:
                summaryReportData.ColorPalette = JsonConvert.DeserializeObject<ColorPalettes>(sortedData[i].Item2);
                break;
              case keyImportedFiles:
                summaryReportData.ImportedFiles = JsonConvert.DeserializeObject<ImportedFileDescriptorListResult>(sortedData[i].Item2);
                break;
              case keyMachineDesigns:
                summaryReportData.DesignData = JsonConvert.DeserializeObject<MachineDesignData>(sortedData[i].Item2);
                break;
            }

            if (data != null)
            {
              Enum.TryParse(_composerRequest.ReportRequest.ReportRoutes[i].ReportRouteType, out OptionalSummaryReportRoute reportEnum);
              data.ReportEnum = reportEnum;
              data.MapImage = sortedData[i].Item3;
              data.ReportUrlQueryCollection = sortedData[i].Item4;
              summaryReportData.Data.Add(data);
            }
          }
        }

        _log.LogInformation($"{nameof(SummaryDataGrabber)}.{nameof(GenerateReportsData)} Summary data extracted successfully from response.");
      }
      catch (Exception ex)
      {
        _log.LogError(ex, "SummaryDataGrabber exception: ");
        response.Message = "Internal Server Error";
        response.DataGrabberStatus = (int)HttpStatusCode.InternalServerError;
      }
      return response;
    }

    private NameValueCollection GetQueryParameters(ReportRoute report)
    {
      NameValueCollection queryCollection = null;
      if (Enum.TryParse(report.ReportRouteType, out OptionalSummaryReportRoute reportEnum))
      {
        string[] splitRequest = report.QueryURL.Split('?');
        if (splitRequest.Count() > 1)
        {
          queryCollection = HttpUtility.ParseQueryString(splitRequest[1], Encoding.UTF8);
          if (queryCollection.Any())
          {
            if (reportEnum == OptionalSummaryReportRoute.Volumes)
            {
              var splitMapRequest = report.MapURL.Split('?');
              if (splitMapRequest.Any())
              {
                var queryMapCollection = HttpUtility.ParseQueryString(splitMapRequest[1], Encoding.UTF8);
                queryCollection.Add(SummaryReportConstants.VolumeCalculationType, queryMapCollection[SummaryReportConstants.VolumeCalculationType]);
              }
            }
          }
        }
      }

      return queryCollection;
    }


    private ImageAPIResponse GetImage(DataGrabberRequest request)
    {
      var imgResult = new[] { new byte { } };
      var imageData = GetData(request);
      var contentType = imageData?.Result.Content.Headers.ContentType;
      if (contentType.ToString() == "image/png")
      {
        imgResult = imageData.Result.Content.ReadAsByteArrayAsync().Result;
      }
      return new ImageAPIResponse { DataGrabberResponseCode = (int) imageData.Result.StatusCode, Image = imgResult };
    }

    private void GetVolumeFilters(DataGrabberResponse response, string filterUrl, NameValueCollection queryCollection)
    {
      try
      {
        string apiResponse;
       var filterRequest = new DataGrabberRequest
        {
          CustomHeaders = _composerRequest.CustomHeaders,
          SvcMethod = HttpMethod.Get
        };

        var topurl = DataGrabberHelper.AppendOrUpdateQueryParam(filterUrl, SummaryReportConstants.FilterUid, queryCollection.Get(SummaryReportConstants.TopUid));
        var baseurl = DataGrabberHelper.AppendOrUpdateQueryParam(filterUrl, SummaryReportConstants.FilterUid, queryCollection.Get(SummaryReportConstants.BaseUid));

        if (topurl != null && !topurl.Equals(filterUrl))
        {
          filterRequest.QueryURL = topurl;
          var filterApiResponse = GetData(filterRequest);
          apiResponse = filterApiResponse.Result.Content.ReadAsStringAsync().Result;
          var filterDescriptor = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(apiResponse);
          if (filterDescriptor != null && filterDescriptor.FilterDescriptor?.FilterJson != null)
          {
            var filterDetails = JsonConvert.DeserializeObject<Filter>(filterDescriptor.FilterDescriptor.FilterJson);
            if (filterDetails != null)
            {
              ((SummaryReportDataModel)response.ReportData).Filters.FilterDescriptors.Add(filterDescriptor.FilterDescriptor);
            }
          }
        }

        if (baseurl != null && !baseurl.Equals(filterUrl))
        {
          filterRequest.QueryURL = baseurl;
          var filterApiResponse = GetData(filterRequest);
          apiResponse = filterApiResponse.Result.Content.ReadAsStringAsync().Result;
          var filterDescriptor = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(apiResponse);
          if (filterDescriptor != null && filterDescriptor.FilterDescriptor != null)
          {
            var filterDetails = JsonConvert.DeserializeObject<Filter>(filterDescriptor.FilterDescriptor.FilterJson);
            if (filterDetails != null)
            {
              ((SummaryReportDataModel)response.ReportData).Filters.FilterDescriptors.Add(filterDescriptor.FilterDescriptor);
            }
          }
        }
      }
      catch (Exception ex)
      {
        _log.LogError(ex, $"{nameof(SummaryDataGrabber)}.{nameof(GetVolumeFilters)}: ");
        response.Message = "Internal Server Error";
        response.DataGrabberStatus = (int)HttpStatusCode.InternalServerError;
      }
    }
  }
}
