using System;

namespace TestUtility
{
  /// <summary>
  /// The Msg class is used for writing Console messages. These are picked up by the test run program and logs  
  /// </summary>
  public class Msg
  {
    private const string DASH = "----------------------------------------------------------------------------------------------------------------------";

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
    /// Display the web api request and response
    /// </summary>
    /// <param name="webMethod">http method</param>
    /// <param name="webRequest">Request</param>
    /// <param name="webResponse">Response</param>
    /// <param name="payload"></param>
    public void DisplayWebApi(string webMethod, string webRequest, string webResponse, string payload)
    {
      Console.WriteLine("WebApi Method   :" + webMethod);
      Console.WriteLine("WebApi Request  :" + webRequest.Replace('&', ' '));
      if (!string.IsNullOrEmpty(payload))
      { Console.WriteLine("WebApi Request Body:" + payload.Replace('&', ' ')); }
      if (!string.IsNullOrEmpty(webResponse))
      { Console.WriteLine("WebApi Response :" + webResponse.Replace('&', ' ')); }
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
