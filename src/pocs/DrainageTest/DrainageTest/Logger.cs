using Morph.Services.Core.Interfaces;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Text;

namespace DrainageTest
{
  [Export(typeof(ILogger))]
  internal class Logger : ILogger
  {
    private TraceSource mTraceSource = new TraceSource(nameof(Logger));
    public const string DefaultTraceSourceName = "Logger";

    private void Log(
      TraceEventType eventType,
      string source,
      string message,
      params object[] args)
    {
      if (!this.mTraceSource.Switch.ShouldTrace(eventType))
        return;
      string format = string.Format("{0:s}: {1}: {2}", (object)DateTime.Now, (object)source, (object)message);
      this.mTraceSource.TraceEvent(eventType, 1, format, args);
    }

    protected StringBuilder BuildExceptionMessage(StringBuilder sb, Exception ex)
    {
      if (ex != null)
      {
        sb.AppendFormat("[{0}]: {1} ", (object)ex.GetType().ToString(), (object)ex.Message);
        this.BuildExceptionMessage(sb, ex.InnerException);
      }
      return sb;
    }

    public void LogVerbose(string source, string message, params object[] args)
    {
      this.Log(TraceEventType.Verbose, source, message, args);
    }

    public void LogInfo(string source, string message, params object[] args)
    {
      this.Log(TraceEventType.Information, source, message, args);
    }

    public void LogWarning(string source, string message, params object[] args)
    {
      this.Log(TraceEventType.Warning, source, message, args);
    }

    public void LogError(string source, string message, params object[] args)
    {
      this.Log(TraceEventType.Error, source, message, args);
    }

    public void LogException(
      string source,
      Exception exception,
      string message,
      params object[] args)
    {
      string str = args != null ? string.Format(message, args) : message;
      this.LogError(source, "{0}: {1}", (object)str, (object)this.BuildExceptionMessage(new StringBuilder("exception: "), exception));
    }
  }
}

