using System;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions.Extensions;

namespace VSS.Log4Net.Extensions
{
  public class Log4NetAdapter : ILogger
  {
    private readonly ILog _logger;
    private readonly IHttpContextAccessor _accessor;

    public Log4NetAdapter(string repoName, string loggerName, IHttpContextAccessor accessor)
    {
      _logger = LogManager.GetLogger(repoName, loggerName);
      _accessor = accessor;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
      switch (logLevel)
      {
        case LogLevel.Trace:
          return _logger.IsDebugEnabled;
        case LogLevel.Debug:
          return _logger.IsDebugEnabled;
        case LogLevel.Information:
          return _logger.IsInfoEnabled;
        case LogLevel.Warning:
          return _logger.IsWarnEnabled;
        case LogLevel.Error:
          return _logger.IsErrorEnabled;
        case LogLevel.Critical:
          return _logger.IsFatalEnabled;
        case LogLevel.None:
          return false;
        default:
          throw new ArgumentException($"Unknown log level {logLevel}.", nameof(logLevel));
      }
    }

    public IDisposable BeginScope<TState>(TState state)
    {
      return null;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
        Func<TState, Exception, string> formatter)
    {
      if (!IsEnabled(logLevel))
      {
        return;
      }
      if (formatter == null)
      {
        throw new ArgumentNullException(nameof(formatter));
      }

      var message = formatter(state, exception);
      if (_accessor?.HttpContext?.Items != null)
        if (_accessor.HttpContext.Items.ContainsKey("RequestID"))
          message = "req:" + _accessor.HttpContext.Items["RequestID"] + " " + message;

      switch (logLevel)
      {
        case LogLevel.Trace:
          _logger.Trace(message, exception);
          break;
        case LogLevel.Debug:
          _logger.Debug(message, exception);
          break;
        case LogLevel.Information:
          _logger.Info(message, exception);
          break;
        case LogLevel.Warning:
          _logger.Warn(message, exception);
          break;
        case LogLevel.Error:
          _logger.Error(message, exception);
          break;
        case LogLevel.Critical:
          _logger.Fatal(message, exception);
          break;
        case LogLevel.None:
          break;
        default:
          _logger.Warn($"Encountered unknown log level {logLevel}, writing out as Info.");
          _logger.Info(message, exception);
          break;
      }
    }
  }
}
