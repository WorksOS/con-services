using System;
using log4net;

namespace VSS.Log4Net.Extensions.Extensions
{
  // ReSharper disable once InconsistentNaming
  public static class ILogExtentions
  {
    public static void Trace(this ILog log, string message, Exception exception)
    {
      log.Logger.Log(
        typeof(ILogExtentions).DeclaringType,
        log4net.Core.Level.Trace,
        message,
        exception);
    }

    public static void Trace(this ILog log, string message)
    {
      log.Trace(message, null);
    }
  }
}