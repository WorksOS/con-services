using System;
using System.IO;
using System.Text.RegularExpressions;

namespace WebApiTests.Utilities
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
      message = message == null ? string.Empty : Regex.Replace(message, @"\s+|\n|\r", " ");
      string contents = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logType}] [{contentType}] {message}";
      using (StreamWriter w = File.AppendText("./accepttest.log")) 
      {
        w.WriteLine(contents);
      }
    }
  }
}
