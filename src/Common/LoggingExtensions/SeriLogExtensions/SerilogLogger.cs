using System;
using Microsoft.Extensions.Logging;

namespace VSS.SeriLog.Extensions
{
  public class SerilogLogger<T> : ILogger
  {
    private readonly ILogger _logger;

    public SerilogLogger(ILoggerFactory factory)
    {
      if (factory == null)
      {
        throw new ArgumentNullException(nameof(factory));
      }

      _logger = factory.CreateLogger(typeof(T).Assembly.GetName().Name);
    }
    
    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
      _logger.Log(logLevel, eventId, state, exception, formatter);
    }
  }
}
