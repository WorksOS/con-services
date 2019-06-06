using Microsoft.Extensions.Logging;

namespace VSS.SeriLog.Extensions
{
  public static class LoggerExtensions
  {
    /// <summary>
    /// To save time in this code, we won't bother building a string to log if isn't going to be logged
    /// </summary>
    private static bool? _isTraceEnabled;

    public static bool IsTraceEnabled(this ILogger logger)
    {
      if (!_isTraceEnabled.HasValue)
      {
        _isTraceEnabled = logger.IsEnabled(LogLevel.Trace);
      }

      return _isTraceEnabled.Value;
    }

    public static bool IsTraceEnabled<T>(this ILogger<T> logger)
    {
      if (!_isTraceEnabled.HasValue)
      {
        _isTraceEnabled = logger.IsEnabled(LogLevel.Trace);
      }

      return _isTraceEnabled.Value;
    }
  }
}
