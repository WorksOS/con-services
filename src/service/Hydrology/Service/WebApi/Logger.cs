#if NET_4_7
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Text;
using Morph.Services.Core.Interfaces;

namespace VSS.Hydrology.WebApi
{
  [Export(typeof(ILogger))]
  internal class Logger : ILogger
  {
    private readonly TraceSource _traceSource = new TraceSource(nameof(Logger));
    public const string DefaultTraceSourceName = "Logger";

    private void Log(TraceEventType eventType, string source, string message, params object[] args)
    {
      if (!_traceSource.Switch.ShouldTrace(eventType))
        return;
      string format = $"{(object) DateTime.Now:s}: {(object) source}: {(object) message}";
      _traceSource.TraceEvent(eventType, 1, format, args);
    }

    private StringBuilder BuildExceptionMessage(StringBuilder sb, Exception ex)
    {
      if (ex != null)
      {
        sb.AppendFormat($"[{(object) ex.GetType().ToString()}]: {(object) ex.Message} ");
        BuildExceptionMessage(sb, ex.InnerException);
      }

      return sb;
    }

    public void LogVerbose(string source, string message, params object[] args)
    {
      Log(TraceEventType.Verbose, source, message, args);
    }

    public void LogInfo(string source, string message, params object[] args)
    {
      Log(TraceEventType.Information, source, message, args);
    }

    public void LogWarning(string source, string message, params object[] args)
    {
      Log(TraceEventType.Warning, source, message, args);
    }

    public void LogError(string source, string message, params object[] args)
    {
      Log(TraceEventType.Error, source, message, args);
    }

    public void LogException(
      string source,
      Exception exception,
      string message,
      params object[] args)
    {
      string str = args != null ? string.Format(message, args) : message;
      LogError(source, "{0}: {1}", (object) str,
        (object) this.BuildExceptionMessage(new StringBuilder("exception: "), exception));
    }
  }
}
#endif
