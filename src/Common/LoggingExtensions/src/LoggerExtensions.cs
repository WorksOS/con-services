using Microsoft.Extensions.Logging;

namespace VSS.Log4NetExtensions
{
  public static class LoggerExtensions
  {
    /// <summary>
    /// To save time in this code, we won't bother building a string to log if isn't going to be logged
    /// </summary>
    private static bool? isTraceEnabled = (bool?)null;

    public static bool IsTraceEnabled(this ILogger logger)
    {
      if (!isTraceEnabled.HasValue)
      {
        isTraceEnabled = logger.IsEnabled(LogLevel.Trace);
      }

      return isTraceEnabled.Value;
    }

    public static bool IsTraceEnabled<T>(this ILogger<T> logger)
    {
      if (!isTraceEnabled.HasValue)
      {
        isTraceEnabled = logger.IsEnabled(LogLevel.Trace);
      }

      return isTraceEnabled.Value;
    }

  }
}
