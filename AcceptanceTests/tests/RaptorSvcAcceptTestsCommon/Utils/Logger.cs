using System;
using System.IO;

namespace RaptorSvcAcceptTestsCommon.Utils
{
  /// <summary>
  /// A mini logger class
  /// </summary>
  public static class Logger
  {
    public enum ContentType
    {
      URI,
      HttpMethod,
      Request,
      HttpCode,
      Response,
      Error,
      RequestHeader,
      ResponseHeader
    }

    public static void Error(string message, ContentType contentType)
    {
      WriteEntry(message, "Error", Enum.GetName(typeof(ContentType), contentType));
    }

    public static void Error(Exception ex, ContentType contentType)
    {
      WriteEntry(ex.Message, "Error", Enum.GetName(typeof(ContentType), contentType));
    }

    public static void Warning(string message, ContentType contentType)
    {
      WriteEntry(message, "Warning", Enum.GetName(typeof(ContentType), contentType));
    }

    public static void Info(string message, ContentType contentType)
    {
      WriteEntry(message, "Info", Enum.GetName(typeof(ContentType), contentType));
    }

    private static void WriteEntry(string message, string logType, string contentType)
    {
      var contents = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},[{logType}],[{contentType}],{message}";

      using (var w = File.AppendText("Log.txt"))
      {
        w.WriteLine(contents);
      }

      // Workaround to Teamcity
      File.Copy("Log.txt", "..\\..\\Log.txt", true);
    }
  }
}
