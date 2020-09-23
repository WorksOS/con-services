using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CCSS.WorksOS.Reports.Common.Helpers;
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
	public class GenericDataGrabber
	{
    //protected Log4NetService LogService;
    //protected readonly RequestContext requestContext;

    //private readonly IReportsApiClient apiClient;
    //private readonly TPaaSIdentityTokenApiClientWithCaching tokenApi;
    //private readonly string consumerKey;
    //private readonly string consumerSecret;
    //private const string className = "GenericDataGrabber";

    protected readonly ILogger _log;
    private IServiceExceptionHandler _serviceExceptionHandler;
    private readonly IWebRequest _gracefulClient;
    protected readonly GenericComposerRequest _composerRequest;


    public GenericDataGrabber(ILogger logger, IServiceExceptionHandler serviceExceptionHandler, IWebRequest gracefulClient, 
      GenericComposerRequest composerRequest)
		{
      _log = logger;
      _serviceExceptionHandler = serviceExceptionHandler;
      _gracefulClient = gracefulClient;
      _composerRequest = composerRequest;
    }

		/// <summary>
		/// Get Reports data
		/// </summary>
		public virtual DataGrabberResponse GenerateReportsData()
		{
      throw new NotImplementedException();
   //   DataGrabberResponse response = new DataGrabberResponse();
			//try
			//{
			//	if (IsRequestValid(request))
			//	{
			//		//var reportHeaders = GetReportsColumnHeaders(request.MappingData, request.RequiredColumns, request.UserPreference);
			//		//ServiceClientResponseMsg reportsData = GetDataFromApi(request);
   //       var reportsData = await _gracefulWebRequest.ExecuteRequestRaw(request.QueryURL, request.Headers, request.SvcMethod);
   //       if (reportsData.ResponseStatusCode == (int)HttpStatusCode.OK ||
			//				reportsData.ResponseStatusCode == (int)HttpStatusCode.NoContent) //NotificationAPI is returning NoContent if no data found
			//		{
			//			response.ReportsData = new JModel()
			//			{
			//				//Headers = reportHeaders,
			//				Data = ParseAndGetData(reportsData.ResponseContent.ReadAsStringAsync().Result, request.MappingData, request.RequiredColumns),
			//				Metadata = new Metadata() { ReportTitle = request.ReportTitle, ReportGeneratedTime = DateTime.UtcNow.ToString(), ReportWorkFlow = request.ReportWorkFlow }
			//			};

			//			if (request.MappingData.MappingMetaDataList.Any(x => x.IsCommonData))
			//			{
			//				var commonData = DataGrabberHelper.GetCommonData(reportsData.ResponseContent.ReadAsStringAsync().Result, request.MappingData, request.RequiredColumns);
			//				response.ReportsData.Data = DataGrabberHelper.AppendCommonData(response.ReportsData.Data, commonData);
			//			}
			//			response.DataGrabberStatus = (int)HttpStatusCode.OK;
			//		}
			//		else
			//		{
			//			var responseContent = reportsData.ResponseContent.ReadAsStringAsync().Result;

			//			if (!string.IsNullOrEmpty(responseContent))
			//			{
			//				response.Message = $"An error occurred invoking the data API: {responseContent}";
			//			}

			//			response.DataGrabberStatus = reportsData.ResponseStatusCode;
			//		}
			//	}
			//	else
			//		response.DataGrabberStatus = (int)HttpStatusCode.InternalServerError; //since validation checks only for mapping data
			//}
			//catch (Exception ex)
			//{
			//	_log.LogError(ex, requestContext, className);
			//	response.Message = "Internal Server Error";
			//	response.DataGrabberStatus = (int)HttpStatusCode.InternalServerError;
			//}
			//return response;
		}

    //protected DataGrabberRequest CloneRequestObject(GenericComposerRequest composerRequest)
    //{
    //  var request = new DataGrabberRequest
    //  {
    //    CustomHeaders = composerRequest.CustomHeaders,
    //    ReportRoutes = composerRequest.ReportRequest.ReportRoutes
    //  };


    //  return request;
    //}

    protected void MapMandatoryResponseProperties(DataGrabberResponse response, IReadOnlyDictionary<string, string> parsedData, MandatoryReportData mandatoryReportData)
    {
      if (parsedData.ContainsKey("ProjectName") && parsedData["ProjectName"] != null)
      {
        mandatoryReportData.ProjectName = JsonConvert.DeserializeObject<ProjectV6DescriptorsSingleResult>(parsedData["ProjectName"]);
      }

      if (parsedData.ContainsKey("ProjectExtents") && parsedData["ProjectExtents"] != null)
      {
        mandatoryReportData.ProjectExtents =
          JsonConvert.DeserializeObject<ProjectStatisticsResult>(parsedData["ProjectExtents"]);
      }

      mandatoryReportData.Filters = new FilterListData { filterDescriptors = new List<FilterDescriptor>() };

      if (parsedData.ContainsKey("Filter") && parsedData["Filter"] != null)
      {
        var filterDescriptor = JsonConvert.DeserializeObject<FilterDescriptorSingleResult>(parsedData["Filter"]);
        if (filterDescriptor?.FilterDescriptor?.FilterJson != null)
        {
          var filterDetails = JsonConvert.DeserializeObject<Filter>(filterDescriptor.FilterDescriptor.FilterJson);
          if (filterDetails != null)
          {
            mandatoryReportData.ReportFilter = filterDetails;
            mandatoryReportData.Filters.filterDescriptors.Add(filterDescriptor.FilterDescriptor);
          }
        }
      }

      if (parsedData.ContainsKey("ImportedFiles") && parsedData["ImportedFiles"] != null)
      {
        mandatoryReportData.ImportedFiles = JsonConvert.DeserializeObject<ImportedFileDescriptorListResult>(parsedData["ImportedFiles"]);
      }
    }

    #region private methods

    ///// <summary>
    ///// check if request is valid
    ///// </summary>
    ///// <param name="request"></param>        
    ///// <returns></returns>
    //public bool IsRequestValid(DataGrabberRequest request)
    //{
    //	bool isValid = true;
    //	if (request.MappingData?.MappingMetaDataList == null || request.MappingData.MappingMetaDataList.Count == 0)
    //	{
    //		LogService.Info(requestContext, className, "Mapping Information not available");
    //		return false;
    //	}
    //	//Will be handled in Controller, there might be a possibility that empty required columns will mean default set
    //	//else if (request.RequiredColumns == null || request.RequiredColumns.Count == 0)
    //	//{
    //	//    isValid = false;
    //	//    LogService.Info(requestContext, className, "Required Columns cannot be empty");
    //	//}
    //	var uiKeys = request.MappingData.MappingMetaDataList.Select(x => x.uiKey).ToList();
    //	var extraColumns = request.RequiredColumns.Except(uiKeys);
    //	if (extraColumns.Count() > 0)
    //		LogService.Fatal(requestContext, className, $"Columns with no respective mapping found {string.Join(" ", extraColumns)}");
    //	return isValid;
    //}

    protected async Task<string> GetData(DataGrabberRequest request)
    {
      var response = await _gracefulClient.ExecuteRequestRaw(request.QueryURL, request.CustomHeaders, request.SvcMethod);

      var code = response.StatusCode;
      var result = response.Content.ReadAsStringAsync().Result;
      if (code != HttpStatusCode.OK)
      {
        _log.LogError($"{nameof(GenerateReportsData)}: Invalid response code {code} and result {result} for reportRoute {request.QueryURL}");
        return string.Empty;
      }

      return !string.IsNullOrEmpty(result)
        ? result
        : string.Empty;
    }

    ///// <summary>
    ///// Get response content from api
    ///// </summary>
    //public ServiceClientResponseMsg GetDataFromApi(DataGrabberRequest request)
    //{
    //	//bool isEndUserContext = true;
    //	//var appCreds = new IdentityApplicationCredentials() { ConsumerKey = consumerKey, ConsumerSecret = consumerSecret };
    //	//if (string.IsNullOrEmpty(request.AccessToken))
    //	//{
    //	//	//Should generate application token only if user access token is not available
    //	//	isEndUserContext = false;
    //	//	request.AccessToken = GetApplicationToken(appCreds);
    //	//}
    //	//request.Headers.Add(new KeyValuePair<string, string>("X-JWT-Assertion", "eyJhbGciOiJSUzI1NiJ9.ew0KICAgICAgICAgICAgICAgICAgICAgICJpc3MiOiAiaHR0cHM6Ly9pZGVudGl0eS1zdGcudHJpbWJsZS5jb20iLA0KCQkJCQkgICJleHAiOiAxNDcyMTI5MzQ2NjIwLA0KCQkJCQkgICJzdWIiOiAiZGluZXNoa3VtYXJfamF5YXJhbWFuQHRyaW1ibGUuY29tIiwNCgkJCQkJICAiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hY2NvdW50bmFtZSI6ICJ0cmltYmxlLmNvbSIsDQoJCQkJCSAgImF6cCI6ICJxeENDeWZSTFpSV3VJdWs5aGJVc0ExdXR5MGNhIiwNCgkJCQkJICAiaHR0cDovL3dzbzIub3JnL2NsYWltcy9maXJzdG5hbWUiOiAiRGluZXNoIEt1bWFyIiwNCgkJCQkJICAiaHR0cDovL3dzbzIub3JnL2NsYWltcy9wYXNzd29yZFBvbGljeSI6ICJISUdIIiwNCgkJCQkJICAiaHR0cDovL3dzbzIub3JnL2NsYWltcy9sb2NhbGl0eSI6ICJUYXJhbWFuaSIsDQoJCQkJCSAgImF0X2hhc2giOiAiejUzdGlVTHlrZGR6TENCMThuMkFXQSIsDQoJCQkJCSAgImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcmVnaW9uIjogIkNoZW5uYWkiLA0KCQkJCQkgICJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RuYW1lIjogIkpheWFyYW1hbiIsDQoJCQkJCSAgImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGVsZXBob25lIjogIjEyMzQ1Njc4OTAiLA0KCQkJCQkgICJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3V1aWQiOiAiZWFjMWE4NDgtNTljYi00ZTJjLTkwN2YtODQxODBlNDk2YmMzIiwNCgkJCQkJICAiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbm5hbWUiOiAiQWxwaGEtVmlzaW9uTGlua0FkbWluaXN0cmF0b3IiLA0KCQkJCQkgICJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjogIkFQUExJQ0FUSU9OIiwNCiAgICAgICAgICAgICAgICAgICAgICAiaWF0IjogMTQ3ODY4NTQ3NiwNCgkJCQkJICAiaHR0cDovL3dzbzIub3JnL2NsYWltcy9zdGF0ZW9ycHJvdmluY2UiOiAiVEFNSUxOQURVIiwNCgkJCQkJICAiaHR0cDovL3dzbzIub3JnL2NsYWltcy9naXZlbm5hbWUiOiAiRGluZXNoIEt1bWFyIiwNCgkJCQkJICAiYXV0aF90aW1lIjogMTQ3ODY4NDY1NSwNCgkJCQkJICAiaHR0cDovL3dzbzIub3JnL2NsYWltcy96aXBjb2RlIjogIjYwMDExMyIsDQoJCQkJCSAgImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvc3RyZWV0YWRkcmVzcyI6ICIyM3JkIHN0IiwNCgkJCQkJICAiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS9hY2NvdW50TG9ja2VkIjogImZhbHNlIiwNCgkJCQkJICAiaHR0cDovL3dzbzIub3JnL2NsYWltcy9jb3VudHJ5IjogIjEyIiwNCgkJCQkJICAiYXVkIjogWw0KCQkJCQkJInF4Q0N5ZlJMWlJXdUl1azloYlVzQTF1dHkwY2EiDQoJCQkJCSAgXSwNCgkJCQkJICAiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbWFpbGFkZHJlc3MiOiAiZGluZXNoa3VtYXJfamF5YXJhbWFuQHRyaW1ibGUuY29tIiwNCgkJCQkJICAiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hY2NvdW50dXNlcm5hbWUiOiAiZGluZXNoa3VtYXJfamF5YXJhbWFuIg0KCQkJCQl9.XP2lgJZk9pz2prmOM2scRSFCIc8gGz-kp52KB5_EzTyKXCEU-KGS8tYE71FSiWYihtrnlP1b0wK7s3LQXz4aHxLzdPaxZropCfLatdz_6SAPnCzAGfOI5lSzdTgnucGlW56B_tB7BamqWN951Ww4-uOZ1AG5F5voMwdbjeyHLCI"));
    //	List<KeyValuePair<string, string>> headers = null;
    //	if (request.Headers != null)
    //	{
    //		headers = new List<KeyValuePair<string, string>>();
    //		headers.AddRange(request.Headers);
    //	}
    //	//var payload = ((request.SvcBody == null) || request.SvcBody.Value.ToString() == "null") ? string.Empty : request.SvcBody.ToString();//To Fix issue where request through api manager assigns JRaw object as null string
    //	var response = apiClient.GetReportsData(new ReportsRequestMsg() { ReportsApiURL = request.QueryURL, Headers = headers, ReportsApiMethod = request.SvcMethod, ReportsPayload = payload }, request.AccessToken);
    //	//if (response.ResponseStatusCode == (int)HttpStatusCode.Unauthorized && !isEndUserContext)
    //	//{
    //	//	//Retry only if application token is used
    //	//	LogService.Info(requestContext, className, "Generating application token again to retry");
    //	//	request.AccessToken = GetApplicationToken(appCreds, true);

    //	//	//reassigning headers with a different name since it gets changed with first apiClient call as pointer is referencing the same
    //	//	List<KeyValuePair<string, string>> reqHeaders = null;
    //	//	if (request.Headers != null)
    //	//	{
    //	//		reqHeaders = new List<KeyValuePair<string, string>>();
    //	//		reqHeaders.AddRange(request.Headers);
    //	//	}

    //	//	response = apiClient.GetReportsData(new ReportsRequestMsg() { ReportsApiURL = request.QueryURL, Headers = reqHeaders, ReportsApiMethod = request.SvcMethod, ReportsPayload = payload }, request.AccessToken);
    //	//}
    //	return response;
    //}

    ///// <summary>
    ///// Gets a DataOcean item.
    ///// </summary>
    ///// <typeparam name="T">The type of item to get</typeparam>
    ///// <param name="route">The route for the request</param>
    ///// <param name="queryParameters">Query parameters for the request</param>
    ///// <param name="customHeaders"></param>
    //private async Task<T> GetDataFromApi<T>(string route, IDictionary<string, string> queryParameters, IHeaderDictionary customHeaders)
    //{
    //  _log.LogDebug($"{nameof(GetData)}: route={route}, queryParameters={JsonConvert.SerializeObject(queryParameters)}");

    //  var query = $"{_dataOceanBaseUrl}{route}";
    //  if (queryParameters != null)
    //  {
    //    query = QueryHelpers.AddQueryString(query, queryParameters);
    //  }
    //  var result = await _gracefulClient.ExecuteRequest<T>(query, null, customHeaders, HttpMethod.Get);
    //  _log.LogDebug($"{nameof(GetData)}: result={(result == null ? "null" : JsonConvert.SerializeObject(result))}");
    //  return result;
    //}

  //  /// <summary>
  //  /// Parse reports api data and generate JModel data based on the mapping information
  //  /// </summary>
  //  /// <param name="reportsData"></param>
  //  /// <param name="mappingData"></param>
  //  /// <param name="requiredColumns"></param>
  //  /// <returns></returns>
  //  public List<Dictionary<string, string>> ParseAndGetData(string reportsData, MappingConfiguration mappingData, List<string> requiredColumns)
		//{
		//	List<Dictionary<string, string>> JModeldata = new List<Dictionary<string, string>>();
		//	if (!string.IsNullOrEmpty(reportsData))
		//	{
		//		LogService.Info(requestContext, className, "Parsing response from API to generate report data");
		//		JArray reportsDataArray;
		//		string rootObject = mappingData.ArrayRootObject;
		//		if (!reportsData.StartsWith("["))
		//		{
		//			JObject output = JObject.Parse(reportsData);
		//			reportsDataArray = new JArray { output };
		//			rootObject = string.Concat(".", mappingData.ArrayRootObject);
		//		}
		//		else
		//			reportsDataArray = JArray.Parse(reportsData);
		//		if (!string.IsNullOrEmpty(rootObject))
		//		{
		//			LogService.Info(requestContext, className, "mappingData:" + rootObject);
		//		}
		//		List<JToken> reportData = string.IsNullOrEmpty(rootObject)
		//			? reportsDataArray.ToList()
		//			: DataGrabberHelper.GetElements(rootObject, reportsDataArray);

		//		if (reportData != null)
		//		{
		//			foreach (JToken record in reportData)
		//			{
		//				Dictionary<string, string> temp = (from metaData in mappingData.MappingMetaDataList
		//												   join column in requiredColumns on metaData.uiKey equals column
		//												   select new
		//												   {
		//													   data = DataGrabberHelper.GetValueBasedonMapping(metaData.ApiMetadata, metaData.dataFormat, record, metaData.IsBlank),
		//													   key = metaData.jModelKey
		//												   }).ToDictionary(kvp => kvp.key, kvp => kvp.data);

		//				JModeldata.Add(temp);
		//			}
		//		}
		//	}
		//	else
		//	{
		//		LogService.Info(requestContext, className, "Received empty response from API");
		//	}
		//	return JModeldata;
		//}

		//public Dictionary<string, string> ParseAndGetSummaryData(string reportsData, MappingConfiguration mappingData)
		//{
		//	Dictionary<string, string> JModelSummarydata = new Dictionary<string, string>();
		//	if (!string.IsNullOrEmpty(reportsData))
		//	{
		//		LogService.Info(requestContext, className, "Parsing response from API to generate summary data");
		//		JArray reportsDataArray;
		//		string summaryObject = mappingData.SummaryMetaData.RootObject;
		//		if (!reportsData.StartsWith("["))
		//		{
		//			JObject output = JObject.Parse(reportsData);
		//			reportsDataArray = new JArray { output };
		//			summaryObject = string.Concat(".", mappingData.SummaryMetaData.RootObject);
		//		}
		//		else
		//			reportsDataArray = JArray.Parse(reportsData);
		//		if (!string.IsNullOrEmpty(summaryObject))
		//		{
		//			LogService.Info(requestContext, className, "mappingData:" + summaryObject);
		//		}

		//		JToken summaryData = DataGrabberHelper.GetValueFromJToken(summaryObject, reportsDataArray);
		//		if (summaryData != null)
		//		{
		//			JModelSummarydata = JsonConvert.DeserializeObject<Dictionary<string, string>>(summaryData.ToString());
		//		}
		//	}
		//	else
		//	{
		//		LogService.Info(requestContext, className, "Received empty summary response from API");
		//	}
		//	return JModelSummarydata;
		//}

		///// <summary>
		///// Get Column headers to be displayed in Report
		///// </summary>
		//public Dictionary<string, string> GetReportsColumnHeaders(MappingConfiguration mappingConfig, List<string> requiredColumns, Preferences userPreference)
		//{
		//	LogService.Info(requestContext, className, "Retriving column headers for report");
		//	Dictionary<string, string> columnHeaders = new Dictionary<string, string>();
		//	if (requiredColumns != null && requiredColumns.Count > 0)
		//	{
		//		columnHeaders = (from column in requiredColumns
		//						 join metaData in mappingConfig.MappingMetaDataList on column equals metaData.uiKey
		//						 select new
		//						 {
		//							 Key = metaData.jModelKey,
		//							 Value = AppendUnitsToColumnHeader(metaData, userPreference)
		//						 }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		//	}
		//	return columnHeaders;
		//}

		///// <summary>
		///// Get Column values(records) to be displayed in Report
		///// </summary>
		///// <param name="mappingConfig"></param>
		/////  <param name="responseData"></param>
		/////  <param name="userPreference"></param>
		///// <returns></returns>
		//public List<Dictionary<string, string>> GetReportsColumnValues(MappingConfiguration mappingConfig, List<Dictionary<string, string>> responseData, Preferences userPreference)
		//{
		//	LogService.Info(requestContext, className, "Retriving column headers for report");

		//	if (responseData != null && responseData.Count > 0)
		//	{
		//		responseData = ApplyLanguagePreferenceToValues(mappingConfig, responseData, userPreference);
		//	}

		//	return responseData;
		//}

		//private static string ApplyUnitType(Dictionary<string, string> responseData)
		//{
		//	string unitType;
		//	var smuType = responseData[Constants.SMU_TYPE];

		//	if (smuType == Constants.ODOMETER)
		//		unitType = Constants.ODOMETER;
		//	else if (smuType == Constants.FUEL)
		//		unitType = Constants.VOLUME;
		//	else
		//		unitType = smuType;

		//	return unitType;
		//}

		//string AppendUnitsToColumnHeader(MappingMetaData mappingData, Preferences userPreference)
		//{
		//	if (userPreference == null)
		//	{
		//		userPreference = new Preferences()
		//		{
		//			Language = mappingData.ApiMetadata.FirstOrDefault().DefaultLanguage,
		//			Units = mappingData.ApiMetadata.FirstOrDefault().DefaultUnits
		//		};

		//		LogService.Info(requestContext, className, $"Userpreference is null assigned dafault values from api, Lang:{userPreference.Language} Unit:{userPreference.Units}");
		//	}
		//	else if (!DataGrabberHelper.IsCultureInfoValid(userPreference.Language))
		//	{
		//		LogService.Info(requestContext, className, $"Language {userPreference.Language} in UserPreference is Invalid assigning dafault value");
		//		userPreference = new Preferences() { Language = Constants.DEFAULT_LANGUAGE };
		//	}

		//	string headerValue;

		//	// If user prefered language resource is not found, fallback to default language resource (en-US). If en-US itself is not found, fallback to uiName
		//	if (mappingData.AppendUnitsInHeader)
		//	{
		//		string resStr1 = ColumnHeaders.ResourceManager.GetString(mappingData.jModelKey, new CultureInfo(userPreference.Language));

		//		if ((resStr1 == null) && !string.Equals(userPreference.Language, Constants.DEFAULT_LANGUAGE))
		//			resStr1 = ColumnHeaders.ResourceManager.GetString(mappingData.jModelKey, new CultureInfo(Constants.DEFAULT_LANGUAGE));

		//		string resStrUnits = null;
		//		if (string.Equals(mappingData.ApiMetadata.FirstOrDefault().FieldUnitType, Constants.CURRENCY, StringComparison.InvariantCultureIgnoreCase))
		//		{
		//			if (string.IsNullOrEmpty(userPreference.CurrencySymbol))
		//				userPreference.CurrencySymbol = Constants.DEFAULT_CURRENCY;
		//			resStrUnits = DataGrabberHelper.GetCurrencyToDisplay(userPreference.CurrencySymbol);
		//		}
		//		else
		//		{
		//			resStrUnits = DataGrabberHelper.GetUnitToDisplay(mappingData.ApiMetadata.FirstOrDefault().FieldUnitType, userPreference.Units, userPreference.Language);

		//			if ((resStrUnits == null) && !string.Equals(userPreference.Language, Constants.DEFAULT_LANGUAGE))
		//				resStrUnits = DataGrabberHelper.GetUnitToDisplay(mappingData.ApiMetadata.FirstOrDefault().FieldUnitType, userPreference.Units, Constants.DEFAULT_LANGUAGE);
		//		}


		//		// TO DO: Handle exception when string format fails
		//		headerValue = ((resStr1 == null) && (resStrUnits == null)) ? mappingData.uiName : string.Format(mappingData.ColumnHeaderFormat, resStr1, resStrUnits);
		//	}
		//	else
		//	{
		//		string resStr1 = null;
		//		if (userPreference.Language != Constants.DEFAULT_LANGUAGE)
		//			resStr1 = ColumnHeaders.ResourceManager.GetString(mappingData.jModelKey, new CultureInfo(userPreference.Language));

		//		if ((resStr1 == null) && !string.Equals(userPreference.Language, Constants.DEFAULT_LANGUAGE))
		//			resStr1 = ColumnHeaders.ResourceManager.GetString(mappingData.jModelKey, new CultureInfo(Constants.DEFAULT_LANGUAGE));

		//		headerValue = resStr1 ?? mappingData.uiName;
		//	}

		//	return headerValue;
		//}

		///// <summary>
		///// Get access token in application context to be used for getting Reports API
		///// </summary>
		//string GetApplicationToken(IdentityApplicationCredentials appCreds, bool forceRefresh = false)
		//{
		//	string accesstoken = null;
		//	var tokenResponse = tokenApi.RetrieveApplicationToken(appCreds, forceRefresh);
		//	if (tokenResponse.ResponseStatusCode == (int)HttpStatusCode.OK)
		//	{
		//		var oAuthToken = tokenResponse.ResponseContent.ReadAsAsync<OAuth2Token>().Result;
		//		accesstoken = oAuthToken.access_token;
		//	}
		//	return accesstoken;
		//}

		///// <summary>
		///// ApplyLanguagePreferenceToValues
		///// </summary>
		///// <param name="mappingConfig"></param>
		///// <param name="responseDataList"></param>
		///// <param name="userPreference"></param>
		///// <returns></returns>
		//List<Dictionary<string, string>> ApplyLanguagePreferenceToValues(MappingConfiguration mappingConfig, List<Dictionary<string, string>> responseDataList, Preferences userPreference)
		//{
		//	var newRespDataList = new List<Dictionary<string, string>>(); var sw = new Stopwatch();
		//	if (userPreference == null)
		//	{
		//		var mappingMetaData = mappingConfig.MappingMetaDataList.FirstOrDefault();
		//		var apiMetaData = mappingMetaData?.ApiMetadata.FirstOrDefault();

		//		if (apiMetaData != null)
		//		{
		//			userPreference = new Preferences
		//			{
		//				Language = apiMetaData.DefaultLanguage,
		//				Units = apiMetaData.DefaultUnits
		//			};
		//			LogService.Info(requestContext, className, $"Userpreference is null assigned dafault values from api, Lang:{userPreference.Language} Unit:{userPreference.Units}");
		//		}
		//	}
		//	else if (!DataGrabberHelper.IsCultureInfoValid(userPreference.Language))
		//	{
		//		LogService.Info(requestContext, className, $"Language {userPreference.Language} in UserPreference is Invalid assigning dafault value");
		//		userPreference = new Preferences { Language = Constants.DEFAULT_LANGUAGE };
		//	}
		//	sw.Start();
		//	foreach (var responseData in responseDataList)
		//	{
		//		// string smuType = responseData["smuType"];//added to handle hybrid units
		//		var s = new Dictionary<string, string>();
		//		foreach (var res in responseData)
		//		{
		//			var mappingData = mappingConfig.MappingMetaDataList.Find(f => f.jModelKey == res.Key);
		//			if (mappingData == null) continue;
		//			var apiMetaData = mappingData.ApiMetadata.FirstOrDefault();
		//			if (apiMetaData == null) continue;
		//			string unitType = apiMetaData.FieldUnitType;

		//			//Apply Specific UnitType only for Hybrid
		//			if (apiMetaData.FieldUnitType == Constants.HYBRID)
		//			{
		//				unitType = ApplyUnitType(responseData);
		//			}

		//			string headerValue;

		//			// If user prefered language resource is not found, fallback to default language resource (en-US). If en-US itself is not found, fallback to uiName
  //                  if(!mappingData.ApplyLangPreferenceForValues.HasValue)
  //                  {
  //                      string resStr1 = ColumnValues.ResourceManager.GetString(res.Value,
  //                          new CultureInfo(userPreference.Language));

  //                      if ((resStr1 == null) &&
  //                          !string.Equals(userPreference.Language, Constants.DEFAULT_LANGUAGE))
  //                          resStr1 = ColumnValues.ResourceManager.GetString(res.Value,
  //                              new CultureInfo(Constants.DEFAULT_LANGUAGE));

  //                      headerValue = resStr1 ?? res.Value;
  //                  }
		//			else if (mappingData.ApplyLangPreferenceForValues.Value)
		//			{
		//				string resStr1 = ColumnValues.ResourceManager.GetString(res.Value, new CultureInfo(userPreference.Language));

		//				if ((resStr1 == null) &&
		//					!string.Equals(userPreference.Language, Constants.DEFAULT_LANGUAGE))
		//					resStr1 = ColumnValues.ResourceManager.GetString(res.Value,
		//						new CultureInfo(Constants.DEFAULT_LANGUAGE));

		//				var resStrUnits = DataGrabberHelper.GetUnitToDisplay(
		//					unitType, userPreference.Units,
		//					userPreference.Language);

		//				if ((resStrUnits == null) &&
		//					!string.Equals(userPreference.Language, Constants.DEFAULT_LANGUAGE))
		//					resStrUnits =
		//						DataGrabberHelper.GetUnitToDisplay(
		//							unitType, userPreference.Units,
		//							Constants.DEFAULT_LANGUAGE);


		//				// TO DO: Handle exception when string format fails
		//				resStr1 = resStr1 ?? res.Value;
		//				headerValue = ((resStr1 == null))
		//					? res.Value
		//					: string.Format(mappingData.ColumnHeaderFormat, resStr1, resStrUnits);
		//			}
		//			else
		//			{
		//				headerValue = res.Value;
		//			}
		//			s.Add(res.Key, headerValue);
		//		}
		//		newRespDataList.Add(s);
		//	}
		//	LogService.Info(requestContext, className, sw.ElapsedMilliseconds + " milliseconds taken for processing " + responseDataList.Count + " Records ");
		//	sw.Stop();
		//	return newRespDataList;
		//}
		#endregion
	}
}
