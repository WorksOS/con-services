using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;


namespace TestUtility
{
  public class TestSupport
  {
    #region Public Properties

    public Guid CustomerUid { get; set; }
    public DateTime EventDate { get; set; }
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
 
    public string CallSchedulerWebApi(string routeSuffix, string method, string body = null, HttpStatusCode expectedCode = HttpStatusCode.OK)
    {
      var uri = GetBaseUri() + routeSuffix;
      var restClient = new RestClientUtil();
      var response = restClient.DoHttpRequest(uri, method, body, expectedCode, "application/json", CustomerUid.ToString());
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
