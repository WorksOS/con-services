using Microsoft.Extensions.Logging;
using System;
using LogLevel = Apache.Ignite.Core.Log.LogLevel;

namespace VSS.TRex.Logging
{
  /// <summary>
  /// Provides an ILogger implementation that wraps a dependency injected ILog interface to allow the Ignite layer
  /// to log into the standard logging location from the Java and .Net layers.
  /// </summary>
  public class TRexIgniteLogger : Apache.Ignite.Core.Log.ILogger
  {
    /// Wrapped logger.
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the TRexIgniteLogger class.
    /// </summary>
    public TRexIgniteLogger() : this(Logging.Logger.CreateLogger<TRexIgniteLogger>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the TRexIgniteLogger class with the provided ILog interface
    /// </summary>
    /// <param name="logger"></param>
    public TRexIgniteLogger(Microsoft.Extensions.Logging.ILogger logger)
    {
      _logger = logger;
    }

    /// <summary>Logs the specified message.</summary>
    /// <param name="level">The level.</param>
    /// <param name="message">The message.</param>
    /// <param name="args">The arguments to format <paramref name="message" />.
    /// Can be null (formatting will not occur).</param>
    /// <param name="formatProvider">The format provider. Can be null if <paramref name="args" /> is null.</param>
    /// <param name="category">The logging category name.</param>
    /// <param name="nativeErrorInfo">The native error information.</param>
    /// <param name="ex">The exception. Can be null.</param>
    public void Log(LogLevel level, string message, object[] args, IFormatProvider formatProvider, string category,
      string nativeErrorInfo, Exception ex)
    {
      object obj = args == null ? message : null;

      _logger.Log<object>(ConvertLogLevel2(level), new EventId(0, ""), obj, ex,
        (_state, _ex) => ex != null ? $"{_state}, Exception {_ex}" : $"{_state}");

      /*
          /// <summary>Writes a log entry.</summary>
          /// <param name="logLevel">Entry will be written on this level.</param>
          /// <param name="eventId">Id of the event.</param>
          /// <param name="state">The entry to be written. Can be also an object.</param>
          /// <param name="exception">The exception related to this entry.</param>
          /// <param name="formatter">Function to create a <c>string</c> message of the <paramref name="state" /> and <paramref name="exception" />.</param>
          void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter);
       */
    }

    /// <summary>
    /// Determines whether the specified log level is enabled.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <returns>
    /// Value indicating whether the specified log level is enabled
    /// </returns>
    public bool IsEnabled(Apache.Ignite.Core.Log.LogLevel level)
    {
      return _logger?.IsEnabled(ConvertLogLevel2(level)) ?? false;
    }

    /// <summary>
    /// Converts the Ignite LogLevel to the log4net log level.
    /// </summary>
    /// <param name="level">The Ignite log level.</param>
    /// <returns>Corresponding log4net log level.</returns>
    public static Microsoft.Extensions.Logging.LogLevel ConvertLogLevel2(Apache.Ignite.Core.Log.LogLevel level)
    {
      switch (level)
      {
        case Apache.Ignite.Core.Log.LogLevel.Trace:
          return Microsoft.Extensions.Logging.LogLevel.Trace;
        case Apache.Ignite.Core.Log.LogLevel.Debug:
          return Microsoft.Extensions.Logging.LogLevel.Debug;
        case Apache.Ignite.Core.Log.LogLevel.Info:
          return Microsoft.Extensions.Logging.LogLevel.Information;
        case Apache.Ignite.Core.Log.LogLevel.Warn:
          return Microsoft.Extensions.Logging.LogLevel.Warning;
        case Apache.Ignite.Core.Log.LogLevel.Error:
          return Microsoft.Extensions.Logging.LogLevel.Error;
        default:
          throw new ArgumentOutOfRangeException(nameof(level), (object) level, (string) null);
      }
    }
  }
}

