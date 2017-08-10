using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TestUtility
{
  public class TestSupport
  {
    #region Public Properties

    public Guid CustomerUid { get; set; }
    public DateTime EventDate { get; set; }
    public bool IsPublishToKafka { get; set; }
    public bool IsPublishToWebApi { get; set; }

    public readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
      DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
      NullValueHandling = NullValueHandling.Ignore
    };

    public readonly TestConfig tsCfg = new TestConfig();

    public TestSupport()
    {
      SetEventDate();
    }
    #endregion

    #region Private Properties

    private readonly Random rndNumber = new Random();
    private readonly object syncLock = new object();
    private const char SEPARATOR = '|';
    private readonly Msg msg = new Msg();

    #endregion

    #region Public Methods
    public void SetEventDate()
    {
      EventDate = DateTime.SpecifyKind(DateTime.Today.AddDays(-RandomNumber(10, 360)), DateTimeKind.Unspecified);
    }
    /// <summary>
    /// Publish events to kafka from string array
    /// </summary>
    /// <param name="eventArray">string array with all the events we are going to publish</param>
    public void PublishEventCollection(string[] eventArray)
    {
      try
      {
        if (IsPublishToWebApi)
        { msg.DisplayEventsToConsoleWeb(eventArray); }
        else if (IsPublishToKafka)
        { msg.DisplayEventsToConsoleKafka(eventArray); }
        else
        { msg.DisplayEventsForDbInjectToConsole(eventArray); }

        var allColumnNames = eventArray.ElementAt(0).Split(SEPARATOR);
        var kafkaDriver = new RdKafkaDriver();
        for (var rowCnt = 1; rowCnt <= eventArray.Length - 1; rowCnt++)
        {
          var eventRow = eventArray.ElementAt(rowCnt).Split(SEPARATOR);
          dynamic dynEvt = ConvertToExpando(allColumnNames, eventRow);
          //var eventDate = dynEvt.EventDate;
          //LastEventDate = eventDate;
          if (IsPublishToKafka || IsPublishToWebApi)
          {
            var jsonString = BuildEventIntoObject(dynEvt);
            //var topicName = SetTheKafkaTopicFromTheEvent(dynEvt.EventType);
            if (IsPublishToWebApi)
            {
              string routeSuffix;
              string method;
              string eventType = dynEvt.EventType;
              switch (eventType)
              {
                case "CreateFilterEvent":
                case "UpdateFilterEvent":
                  routeSuffix = $"api/v4/filter/{dynEvt.ProjectUID}";
                  method = "PUT";
                  break;
                case "DeleteFilterEvent":
                  routeSuffix = $"api/v4/filter/{dynEvt.ProjectUID}";
                  method = "DELETE";
                  break;
                default:
                  routeSuffix = $"api/v4/filter/{dynEvt.ProjectUID}";
                  method = "GET";
                  break;
              }
              CallFilterWebApi(routeSuffix, method, jsonString);
            }
            else
            {
              //kafkaDriver.SendKafkaMessage(topicName, jsonString);
              //WaitForTimeBasedOnNumberOfRecords(eventArray.Length);
            }
          }
          else
          {
            BuildMySqlInsertStringAndWriteToDatabase(dynEvt);
          }
        }
      }
      catch (Exception ex)
      {
        msg.DisplayException(ex.Message);
        throw;
      }
    }

    public string CallFilterWebApi(string routeSuffix, string method, string body = null)
    {

      var uri = GetBaseUri() + routeSuffix;  // "http://localhost:20979/"
      var restClient = new RestClientUtil();
      var response = restClient.DoHttpRequest(uri, method, body, HttpStatusCode.OK, "application/json", CustomerUid.ToString());
      return response;
    }

    public DateTime ConvertTimeStampAndDayOffSetToDateTime(string timeStampAndDayOffSet, DateTime startEventDateTime)
    {
      var components = Regex.Split(timeStampAndDayOffSet, @"d+\+");
      var offset = double.Parse(components[0].Trim());
      return DateTime.Parse(startEventDateTime.AddDays(offset).ToString("yyyy-MM-dd") + " " + components[1].Trim());
    }

    public string GetBaseUri()
    {
      var baseUri = tsCfg.webApiUri;
      if (Debugger.IsAttached || tsCfg.operatingSystem == "Windows_NT")
      {
        baseUri = tsCfg.debugWebApiUri;
      }
      return baseUri;
    }

    #endregion

    #region "private methods"
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
        case "CreateFilterEvent":
          var createFilterEvent = new CreateFilterEvent
          {
            ActionUTC = EventDate,
            FilterUID = new Guid(eventObject.FilterUID),
            CustomerUID = new Guid(eventObject.CustomerUID),
            ProjectUID = new Guid(eventObject.ProjectUID),
            UserUID = new Guid(eventObject.UserUID),
            FilterJson = eventObject.FilterJson
          };
          jsonString = JsonConvert.SerializeObject(new { CreateFilterEvent = createFilterEvent }, jsonSettings);
          break;
        case "UpdateFilterEvent":
          var updateFilterEvent = new UpdateFilterEvent
          {
            ActionUTC = EventDate,
            FilterUID = new Guid(eventObject.FilterUID),
            CustomerUID = new Guid(eventObject.CustomerUID),
            ProjectUID = new Guid(eventObject.ProjectUID),
            UserUID = new Guid(eventObject.UserUID),
            FilterJson = eventObject.FilterJson            
          };
          jsonString = JsonConvert.SerializeObject(new { UpdateFilterEvent = updateFilterEvent }, jsonSettings);
          break;
        case "DeleteFilterEvent":
          var deleteFilterEvent = new DeleteFilterEvent
          {
            ActionUTC = EventDate,
            FilterUID = new Guid(eventObject.FilterUID),
            CustomerUID = new Guid(eventObject.CustomerUID),
            ProjectUID = new Guid(eventObject.ProjectUID),
            UserUID = new Guid(eventObject.UserUID),            
          };
          jsonString = JsonConvert.SerializeObject(new { DeleteFilterEvent = deleteFilterEvent }, jsonSettings);
          break;
      }
      return jsonString;
    }

    /// <summary>
    /// Inserts the events into the database
    /// </summary>
    /// <param name="eventObject"></param>
    private void BuildMySqlInsertStringAndWriteToDatabase(dynamic eventObject)
    {
      string dbTable = eventObject.TableName;
      var mysqlHelper = new MySqlHelper();
      var sqlCmd = $@"INSERT INTO `{tsCfg.dbSchema}`.{dbTable} ";
      switch (dbTable)
      {
        case "Filter":
          sqlCmd += $@"(FilterUID,fk_CustomerUID,fk_ProjectUID,fk_UserUID,Name,FilterJson,IsDeleted,LastActionedUTC) VALUES 
                ('{eventObject.FilterUID}','{eventObject.fk_CustomerUID}','{eventObject.fk_ProjectUID}','{eventObject.fk_UserUID}','{eventObject.Name}','{eventObject.FilterJson}',{eventObject.IsDeleted},'{eventObject.LastActionedUTC}');";
          break;
      }
      mysqlHelper.ExecuteMySqlInsert(tsCfg.DbConnectionString, sqlCmd);
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
      if (expandoDict.ContainsKey(propertyName) && expandoDict[propertyName] != null)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Create an ExpandoObject of all the fields from the event array
    /// </summary>
    /// <param name="allColumnNames">All the column names from the array</param>
    /// <param name="singleEventRow">A single row of event data</param>
    /// <returns>Object with all properties from array</returns>
    private ExpandoObject ConvertToExpando(string[] allColumnNames, string[] singleEventRow)
    {
      var expObj = new ExpandoObject() as IDictionary<string, Object>;
      var colIdx = -1;
      foreach (var colName in allColumnNames)
      {
        colIdx++;
        if (colName.Trim() == string.Empty)
        { continue; }

        dynamic obj = TransformObject(singleEventRow[colIdx].Trim());
        expObj.Add(colName.Trim(), obj);
      }
      return (ExpandoObject)expObj;
    }

    /// <summary>
    /// For some of the events in the event array they need some transforming 
    /// </summary>
    /// <param name="propertyValue">returns a converted/transformed single property. Mainly used for string null or special dates</param>
    /// <returns></returns>
    private dynamic TransformObject(string propertyValue)
    {
      dynamic obj;
      if (propertyValue == "null" || propertyValue == string.Empty)
      {
        return null;
      }
      if (Regex.IsMatch(propertyValue, @"^\s*\d+d\+\d+"))
      {
        obj = ConvertTimeStampAndDayOffSetToDateTime(propertyValue, EventDate);
        return obj;
      }
      obj = propertyValue;
      return obj;
    }

    /// <summary>
    /// Generate a random number. This is use for the number of days in the past to get a start date from.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    private int RandomNumber(int min, int max)
    {
      lock (syncLock)
      {
        return rndNumber.Next(min, max);
      }
    }

    #endregion

  }
}
