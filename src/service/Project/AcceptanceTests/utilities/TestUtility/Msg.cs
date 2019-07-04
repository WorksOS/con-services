using Serilog;

namespace TestUtility
{
  /// <summary>
  /// The Msg class is used for writing messages to the log and console.
  /// </summary>
  public static class Msg
  {
    private const string DASH = "----------------------------------------------------------------------------------------------------------------------";
    private const string STAR = "**********************************************************************************************************************";
    private const string INJECT = "                            Inject Kafka Events";
    private const string INJECTWEB = "                        Inject Web API messages";
    private const string INJSQL = "                            Inject MySql Records";
    private const string MYSQL = "MySql Query:";
    private const string RSLT = "Result checking:";

    /// <summary>
    /// Console message a desciption of the test and it's purpose.
    /// </summary>
    /// <param name="descriptionOfTest">Consise description of the test</param>
    public static void Title(string descriptionOfTest)
    {
      Log.Information(descriptionOfTest);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void Title(string testTag, string descriptionOfTest)
    {
      Log.Information(STAR);
      Log.Information($"Test Name: {testTag}");
      Log.Information(descriptionOfTest);
      Log.Information(DASH);
    }
    /// <summary>
    /// Display all the events in the console 
    /// </summary>
    public static void DisplayEventsToConsoleKafka(string[] eventArray)
    {
      Log.Information(DASH);
      Log.Information(INJECT);
      Log.Information(DASH);

      foreach (var row in eventArray)
      {
        Log.Information(row);
      }

      Log.Information(DASH);
    }

    /// <summary>
    /// Display all the events in the console 
    /// </summary>
    public static void DisplayEventsToConsoleWeb(string[] eventArray)
    {
      Log.Information(DASH);
      Log.Information(INJECTWEB);
      Log.Information(DASH);
      foreach (var row in eventArray)
      {
        Log.Information(row);
      }
      Log.Information(DASH);
    }

    public static void DisplayEventsForDbInjectToConsole(string[] eventArray)
    {
      Log.Information(DASH);
      Log.Information(INJSQL);
      Log.Information(DASH);
      foreach (var row in eventArray)
      {
        Log.Information(row);
      }
      Log.Information(DASH);
    }

    /// <summary>
    /// Console message the database query
    /// </summary>
    public static void DisplayMySqlQuery(string query)
    {
      Log.Information(MYSQL);
      Log.Information(query);
    }

    /// <summary>
    /// Console message expected and actual
    /// </summary>
    public static void DisplayResults(string expected, string actual)
    {
      Log.Information(RSLT);
      Log.Information($"Expected: {expected}");
      Log.Information($"Actual  : {actual}");
    }

    /// <summary>
    /// Display the web api request and response
    /// </summary>
    public static void DisplayWebApi(string webMethod, string webRequest, string webResponse, string payload)
    {
      Log.Information($"WebApi Method   :{webMethod}");
      Log.Information($"WebApi Request  :{webRequest.Replace('&', ' ')}");

      if (!string.IsNullOrEmpty(payload)) { Log.Information($"WebApi Request Body:{payload.Replace('&', ' ')}"); }
      if (!string.IsNullOrEmpty(webResponse)) { Log.Information($"WebApi Response :{webResponse.Replace('&', ' ')}"); }
    }

    public static void DisplayException(string exception)
    {
      Log.Information(DASH);
      Log.Information($"**** EXCEPTION ****: {exception}");
      Log.Information(DASH);
    }
  }
}
