using System;
using System.IO;

namespace TagFiles
{
  /// <summary>
  /// Log Megalodon Messages
  /// Todo write log messages to disk
  /// </summary>
  public static class MegalodonLogger
  {


    private static void WriteToLog(string msg)
    { 
      string pathString = @"c:\megalodon\log";
      System.IO.Directory.CreateDirectory(pathString);
      pathString = System.IO.Path.Combine(pathString, "servererrorlog.txt");

      using (StreamWriter w = File.AppendText(pathString))
      {
        w.WriteLine($"{DateTime.Now.ToString()} {msg}");
      }
    }

    public static void LogInfo(string msg)
    {
      WriteToLog(msg);
      System.Diagnostics.Trace.WriteLine("Info. "+ msg);
    }

    public static void LogDebug(string msg)
    {
      WriteToLog(msg);
      System.Diagnostics.Debug.WriteLine(msg);
    }

    public static void LogError(string msg)
    {
      WriteToLog(msg);
      System.Diagnostics.Trace.WriteLine("Error. " + msg);
    }

    public static void LogWarning(string msg)
    {
      WriteToLog(msg);
      System.Diagnostics.Trace.WriteLine("Warning. "+msg);
    }

  }
}
