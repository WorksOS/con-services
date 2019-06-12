using System;

namespace TestUtility
{
  /// <summary>
  /// The Msg class is used for writing Console messages. These are picked up by the test run program and logs  
  /// </summary>
  public static class Msg
  {

    private const string DASH = "----------------------------------------------------------------------------------------------------------------------";
    private const string STAR = "**********************************************************************************************************************";
    private const string INJECT = "                            Inject Kafka Events                            ";
    private const string INJECTWEB = "                        Inject Web API messages                         ";
    private const string INJSQL = "                            Inject MySql Records                           ";
    private const string MYSQL = "MySql Query:";
    private const string RSLT = "Result checking:";

    public static string currentTest = string.Empty;

    /// <summary>
    /// Console message a desciption of the test and it's purpose.
    /// </summary>
    /// <param name="descriptionOfTest">Consise description of the test</param>
    public static void Title(string descriptionOfTest)
    {
      Console.WriteLine();
      Console.WriteLine(descriptionOfTest);
      Console.WriteLine();
    }

    /// <summary>
    /// 
    /// </summary>
    public static void Title(string testTag, string descriptionOfTest)
    {
      currentTest = testTag;
      Console.WriteLine(STAR);
      Console.WriteLine("Test Name:" + testTag);
      Console.WriteLine(descriptionOfTest);
      Console.WriteLine(DASH);
    }
    /// <summary>
    /// Display all the events in the console 
    /// </summary>
    public static void DisplayEventsToConsoleKafka(string[] eventArray)
    {
      Console.WriteLine(DASH);
      Console.WriteLine(INJECT);
      Console.WriteLine(DASH);
      foreach (var row in eventArray)
      {
        Console.WriteLine(row);
      }
      Console.WriteLine(DASH);
    }

    /// <summary>
    /// Display all the events in the console 
    /// </summary>
    /// <param name="eventArray"></param>
    public static void DisplayEventsToConsoleWeb(string[] eventArray)
    {
      Console.WriteLine(DASH);
      Console.WriteLine(INJECTWEB);
      Console.WriteLine(DASH);
      foreach (var row in eventArray)
      {
        Console.WriteLine(row);
      }
      Console.WriteLine(DASH);
    }
    public static void DisplayEventsForDbInjectToConsole(string[] eventArray)
    {
      Console.WriteLine(DASH);
      Console.WriteLine(INJSQL);
      Console.WriteLine(DASH);
      foreach (var row in eventArray)
      {
        Console.WriteLine(row);
      }
      Console.WriteLine(DASH);
    }

    /// <summary>
    /// Console message the database query
    /// </summary>
    public static void DisplayMySqlQuery(string query)
    {
      Console.WriteLine(MYSQL);
      Console.WriteLine(query);
      Console.WriteLine();
    }

    /// <summary>
    /// Console message expected and actual
    /// </summary>
    public static void DisplayResults(string expected, string actual)
    {
      Console.WriteLine(RSLT);
      Console.WriteLine("Expected: " + expected);
      Console.WriteLine("Actual  : " + actual);
      Console.WriteLine();
    }

    /// <summary>
    /// Display the web api request and response
    /// </summary>
    public static void DisplayWebApi(string webMethod, string webRequest, string webResponse, string payload)
    {
      Console.WriteLine("WebApi Method   :" + webMethod);
      Console.WriteLine("WebApi Request  :" + webRequest.Replace('&', ' '));
      if (!string.IsNullOrEmpty(payload)) { Console.WriteLine("WebApi Request Body:" + payload.Replace('&', ' ')); }
      if (!string.IsNullOrEmpty(webResponse)) { Console.WriteLine("WebApi Response :" + webResponse.Replace('&', ' ')); }
      Console.WriteLine();
    }

    public static void DisplayException(string exception)
    {
      Console.WriteLine(DASH);
      Console.WriteLine("**** EXCEPTION ****: " + exception);
      Console.WriteLine(DASH);
    }
  }
}
