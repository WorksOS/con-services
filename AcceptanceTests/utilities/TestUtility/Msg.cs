using System;

namespace TestUtility
{
    /// <summary>
    /// The Msg class is used for writing Console messages. These are picked up by the test run program and logs  
    /// </summary>
    public class Msg
    {

      private const string DASH   = "----------------------------------------------------------------------------------------------------------------------"; 
      private const string INJECT = "                            Inject Kafka Events                            "; 
      private const string INJSQL = "                            Inject MySql Records                           "; 
      private const string MYSQL  = "MySql Query:"; 
      private const string RSLT  = "Result checking:"; 

      public string currentTest = string.Empty;
        
      /// <summary>
      /// Console message a desciption of the test and it's purpose.
      /// </summary>
      /// <param name="descriptionOfTest">Consise description of the test</param>
      public void Title(string descriptionOfTest)
      {
          Console.WriteLine();
          Console.WriteLine(descriptionOfTest);
          Console.WriteLine();
      }        
        
      /// <summary>
      /// 
      /// </summary>
      /// <param name="testTag"></param>
      /// <param name="descriptionOfTest"></param>
      public void Title(string testTag,string descriptionOfTest)
      {
          currentTest = testTag;
          Console.WriteLine("Test tag:" + testTag);
          Console.WriteLine(descriptionOfTest);
          Console.WriteLine();
      }    
      /// <summary>
      /// Display all the events in the console 
      /// </summary>
      /// <param name="eventArray"></param>
      public void DisplayEventsToConsole(string[] eventArray)
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

      public void DisplayEventsForDbInjectToConsole(string[] eventArray)
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
      /// <param name="query"></param>
      public void DisplayMySqlQuery(string query)
      {
          Console.WriteLine(MYSQL);
          Console.WriteLine(query);
          Console.WriteLine();                    
      }



      /// <summary>
      /// Console message expected and actual
      /// </summary>
      /// <param name="expected"></param>
      /// <param name="actual"></param>
      public void DisplayResults(string expected, string actual)
      {
          Console.WriteLine(RSLT);
          Console.WriteLine("Expected: " + expected);
          Console.WriteLine("Actual  : " + actual);
          Console.WriteLine();                    
      }

      /// <summary>
      /// Display the web api request and response
      /// </summary>
      /// <param name="webMethod">http method</param>
      /// <param name="webRequest">Request</param>
      /// <param name="webResponse">Response</param>
      /// <param name="payload"></param>
      public void DisplayWebApi(string webMethod, string webRequest, string webResponse, string payload)
      {
          Console.WriteLine("WebApi Method   :" + webMethod);
          Console.WriteLine("WebApi Request  :" + webRequest.Replace('&',' '));
          if (!string.IsNullOrEmpty(payload))
              { Console.WriteLine("WebApi Request Body:" + payload.Replace('&',' '));}
          if (!string.IsNullOrEmpty(webResponse))
              { Console.WriteLine("WebApi Response :" + webResponse.Replace('&',' '));}
          Console.WriteLine();                    
      }

      public void DisplayException(string exception)
      {
        Console.WriteLine(DASH);
        Console.WriteLine("**** EXCEPTION ****: " + exception);
        Console.WriteLine(DASH);
      }
    }
}
