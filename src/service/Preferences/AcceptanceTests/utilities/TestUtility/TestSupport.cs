using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CCSS.Productivity3D.Preferences.Abstractions.ResultsHandling;
using CCSS.Productivity3D.Preferences.Common.Models;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.VisionLink.Interfaces.Events.Preference;
using Xunit;

namespace TestUtility
{
  public class TestSupport
  {
    public Guid CustomerUid { get; set; }

    public readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
      DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
      NullValueHandling = NullValueHandling.Ignore
    };

    private const char SEPARATOR = '|';

    public TestSupport()
    { 
      SetCustomerUid();
    }

    /// <summary>
    /// Set the customer UID to a random GUID
    /// </summary>
    public void SetCustomerUid() => CustomerUid = Guid.NewGuid();
    
        
    /// <summary>
    /// Publish events to web api from string array
    /// </summary>
    public async Task<string> PublishEventCollection<TRes>(string[] eventArray, string queryParams=null, HttpStatusCode statusCode = HttpStatusCode.OK)
      where TRes : ContractExecutionResult
    {
      string jsonString = null;
      try
      {     
        Msg.DisplayEventsToConsoleWeb(eventArray);
        var allColumnNames = eventArray.ElementAt(0).Split(SEPARATOR);
        for (var rowCnt = 1; rowCnt <= eventArray.Length - 1; rowCnt++)
        {
          var eventRow = eventArray.ElementAt(rowCnt).Split(SEPARATOR);
          dynamic dynEvt = ConvertToExpando(allColumnNames, eventRow);
        
          jsonString = BuildEventIntoObject(dynEvt);
          await CallPreferenceWebApi<TRes>(jsonString, dynEvt.EventType, dynEvt.CustomerUID, statusCode, queryParams);           
        }
      }
      catch (Exception ex)
      {
        Msg.DisplayException(ex.Message);
        throw;
      }
      return jsonString;
    }

    /// <summary>
    /// Publish event to web api
    /// </summary>
    public async Task<string> PublishEventToWebApi<TRes>(string[] eventArray, string queryParams=null, HttpStatusCode statusCode = HttpStatusCode.OK)
      where TRes : ContractExecutionResult
    {
      try
      {
        Msg.DisplayEventsToConsoleWeb(eventArray);
        var allColumnNames = eventArray.ElementAt(0).Split(SEPARATOR);
        var eventRow = eventArray.ElementAt(1).Split(SEPARATOR);
        dynamic eventObject = ConvertToExpando(allColumnNames, eventRow);
        var jsonString = BuildEventIntoObject(eventObject);
        string response;
        try
        {
          response = await CallPreferenceWebApi<TRes>(jsonString, eventObject.EventType, eventObject.CustomerUID, statusCode, queryParams);
        }
        catch (RuntimeBinderException)
        {
          response = await CallPreferenceWebApi<TRes>(jsonString, eventObject.EventType, CustomerUid.ToString(), statusCode, queryParams);
        }
        return response;
      }
      catch (Exception ex)
      {
        Msg.DisplayException(ex.Message);
        return ex.Message;
      }
    }

    /// <summary>
    /// Call the preference service web api
    /// </summary>
    private async Task<string> CallPreferenceWebApi<TRes>(string jsonString, string eventType, string customerUid, HttpStatusCode statusCode, string queryParams=null) 
      where TRes : ContractExecutionResult
    {
      var response = string.Empty;

      switch (eventType)
      {
        case "CreateUserPreferenceRequest":
          response = await CallWebApi($"api/v1/user{queryParams}", HttpMethod.Post, jsonString, customerUid, statusCode: statusCode);
          break;
        case "UpdateUserPreferenceRequest":
          response = await CallWebApi("api/v1/user", HttpMethod.Put, jsonString, customerUid, statusCode: statusCode);
          break;
        case "DeleteUserPreferenceRequest":
          response = await CallWebApi($"api/v1/user{queryParams}", HttpMethod.Delete, string.Empty, customerUid, statusCode: statusCode);
          break;
        case "CreatePreferenceKeyEvent":
          response = await CallWebApi("api/v1/user/key", HttpMethod.Post, jsonString, customerUid, statusCode: statusCode);
          break;
        case "UpdatePreferenceKeyEvent":
          response = await CallWebApi("api/v1/user/key", HttpMethod.Put, jsonString, customerUid, statusCode: statusCode);
          break;
        case "DeletePreferenceKeyEvent":
          response = await CallWebApi("api/v1/user/key", HttpMethod.Delete, jsonString, customerUid, statusCode: statusCode);
          break;
      }

      var jsonResponse = JsonConvert.DeserializeObject<TRes>(response);

      return jsonResponse.Message;
    }

    /// <summary>
    /// Call user preference web api and check result. 
    /// Note: no 'get preference key' api so cannot use this method for preference keys only user preferences.
    /// </summary>
    public async Task GetUserPreferenceViaWebApiAndCompareActualWithExpected(HttpStatusCode statusCode, Guid customerUid, string request, bool ignoreZeros)
    {
      var createRequest = JsonConvert.DeserializeObject<UpsertUserPreferenceRequest>(request);
      var queryParams = $"?keyName={createRequest.PreferenceKeyName}&userUid={createRequest.TargetUserUID}";
      var response = await CallWebApi($"api/v1/user{queryParams}", HttpMethod.Get, null, customerUid.ToString());
      if (statusCode == HttpStatusCode.OK)
      {
        var actualPref = JsonConvert.DeserializeObject<UserPreferenceV1Result>(response);
        var expectedPref = ConstructExpectedResult(createRequest);
        Msg.DisplayResults($"Expected preference: {JsonConvert.SerializeObject(expectedPref)}", $"Actual from WebApi: {response}");
          
        CompareTheActualWithExpected(actualPref, expectedPref, ignoreZeros);
      }
    }

    /// <summary>
    /// Compare the actual with the expected
    /// </summary>
    public void CompareTheActualWithExpected<T>(T actual, T expected, bool ignoreZeros)
    {     
      var oType = actual.GetType();
      foreach (var oProperty in oType.GetProperties())
      {
        var expectedValue = oProperty.GetValue(expected, null);
        var actualValue = oProperty.GetValue(actual, null);
        if (ignoreZeros)
        {
          if (expectedValue == null)
          {
            continue;
          }
          if (expectedValue.ToString() == "0")
          {
            continue;
          }
        }

        Assert.Equal(expectedValue, actualValue);
      } 
    }

    /// <summary>
    /// Create an instance of the master data events. Convert to JSON. 
    /// </summary>
    /// <param name="eventObject">event to be published</param>
    /// <returns>json string with event serialized</returns>
    private string BuildEventIntoObject(dynamic eventObject)
    {
      var jsonString = string.Empty;
      string eventType = eventObject.EventType;
      switch (eventType)
      {
        case "CreateUserPreferenceRequest":
        case "UpdateUserPreferenceRequest":
          var request = new UpsertUserPreferenceRequest();
          if (HasProperty(eventObject, "TargetUserUID"))
          {
            request.TargetUserUID = Guid.Parse(eventObject.TargetUserUID);
          }
          if (HasProperty(eventObject, "SchemaVersion"))
          {
            request.SchemaVersion = eventObject.SchemaVersion;
          }
          if (HasProperty(eventObject, "PreferenceJson"))
          {
            request.PreferenceJson = eventObject.PreferenceJson;
          }
          if (HasProperty(eventObject, "PreferenceKeyName"))
          {
            request.PreferenceKeyName = eventObject.PreferenceKeyName;
          }
          if (HasProperty(eventObject, "PreferenceKeyUID"))
          {
            request.PreferenceKeyUID = Guid.Parse(eventObject.PreferenceKeyUID);
          }
          jsonString = JsonConvert.SerializeObject(request, JsonSettings);
          break;   
        case "CreatePreferenceKeyEvent":
          var createPrefKeyEvent = new CreatePreferenceKeyEvent
          {
            PreferenceKeyName = eventObject.PreferenceKeyName,
            PreferenceKeyUID = Guid.Parse(eventObject.PreferenceKeyUID)
          };
          jsonString = JsonConvert.SerializeObject(createPrefKeyEvent, JsonSettings);
          break;
        case "UpdatePreferenceKeyEvent":
          var updatePrefKeyEvent = new UpdatePreferenceKeyEvent
          {
            PreferenceKeyName = eventObject.PreferenceKeyName,
            PreferenceKeyUID = Guid.Parse(eventObject.PreferenceKeyUID)
          };
          jsonString = JsonConvert.SerializeObject(updatePrefKeyEvent, JsonSettings);
          break;
        case "DeletePreferenceKeyEvent":
          var deletePrefKeyEvent = new DeletePreferenceKeyEvent
          {
            PreferenceKeyUID = Guid.Parse(eventObject.PreferenceKeyUID)
          };
          jsonString = JsonConvert.SerializeObject(deletePrefKeyEvent, JsonSettings);
          break;
      }
      return jsonString;
    }
 
    /// <summary>
    /// Check that a property exists in the dynamic object
    /// </summary>
    /// <param name="obj">dynamic object</param>
    /// <param name="propertyName">Property name as string</param>
    /// <returns>true or false</returns>
    private static bool HasProperty(dynamic obj, string propertyName)
    {
      var expandoDict = (IDictionary<string, object>)obj;
      return expandoDict.ContainsKey(propertyName) && expandoDict[propertyName] != null;
    }

    /// <summary>
    /// Create an ExpandoObject of all the fields from the event array
    /// </summary>
    /// <param name="allColumnNames">All the column names from the array</param>
    /// <param name="singleEventRow">A single row of event data</param>
    /// <returns>Object with all properties from array</returns>
    private ExpandoObject ConvertToExpando(string[] allColumnNames, string[] singleEventRow)
    {
      var expObj = new ExpandoObject() as IDictionary<string, object>;
      var colIdx = -1;

      foreach (var colName in allColumnNames)
      {
        colIdx++;
        if (colName.Trim() == string.Empty)
        { continue; }

        var obj = TransformObject(singleEventRow[colIdx].Trim());
        expObj.Add(colName.Trim(), obj);
      }
      return (ExpandoObject)expObj;
    }

    /// <summary>
    /// For some of the events in the event array they need some transforming 
    /// </summary>
    /// <param name="propertyValue">returns a converted/transformed single property. Mainly used for string null or special dates</param>
    private dynamic TransformObject(string propertyValue)
    {
      dynamic obj;
      if (propertyValue == "null" || propertyValue == string.Empty)
      {
        return null;
      }
      /*
      if (Regex.IsMatch(propertyValue, @"^\s*\d+d\+\d+"))
      {
        obj = DateTimeHelper.ConvertTimeStampAndDayOffSetToDateTime(propertyValue, FirstEventDate);
        return obj;
      }
      */
      obj = propertyValue;
      return obj;
    }

    /// <summary>
    /// Use the request to construct the expected result
    /// </summary>
    private UserPreferenceV1Result ConstructExpectedResult(UpsertUserPreferenceRequest request)
    {
      var expectedUserPref = new UserPreferenceV1Result();
      expectedUserPref.PreferenceKeyName = request.PreferenceKeyName;
      expectedUserPref.PreferenceKeyUID = request.PreferenceKeyUID.Value;
      expectedUserPref.SchemaVersion = request.SchemaVersion;
      expectedUserPref.PreferenceJson = request.PreferenceJson;
      return expectedUserPref;
    }

    public Task<string> CallWebApi(string routeSuffix, HttpMethod method, string configJson, string customerUid = null, string jwt = null, HttpStatusCode statusCode = HttpStatusCode.OK)
      => RestClient.SendHttpClientRequest($"{routeSuffix}", method, MediaTypes.JSON, MediaTypes.JSON, customerUid, configJson, jwt, expectedHttpCode: statusCode);
  }
}
