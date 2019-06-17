using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using FrameworkLogger = Microsoft.Extensions.Logging.ILogger;
using ILogger = Serilog.ILogger;

namespace VSS.Serilog.Extensions
{
  /// <summary>
  /// Custom SerilogLogger. Ordinarily we might use serilog.extensions.logging and use the SerilogLogger
  /// but our dependency on IHttpContextAccessor for request ID information means we need to extend it,
  /// and creating a new implementation is easier than trying to work over the top of serilog.extensions.logging package.
  /// </summary>
  class SerilogLogger : FrameworkLogger
  {
    readonly SerilogProvider _provider;
    readonly ILogger _logger;

    static readonly MessageTemplateParser _messageTemplateParser = new MessageTemplateParser();

    public SerilogLogger(
        SerilogProvider provider,
        ILogger logger = null,
        string name = null)
    {
      _provider = provider ?? throw new ArgumentNullException(nameof(provider));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));

      if (name != null)
      {
        _logger = _logger.ForContext(Constants.SourceContextPropertyName, name);
      }
    }

    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(ConvertLevel(logLevel));

    public IDisposable BeginScope<TState>(TState state)
    {
      return _provider.BeginScope(state);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
      var level = ConvertLevel(logLevel);
      if (!_logger.IsEnabled(level))
      {
        return;
      }

      var logger = _logger;
      string messageTemplate = null;

      var properties = new List<LogEventProperty>();

      if (state is IEnumerable<KeyValuePair<string, object>> structure)
      {
        foreach (var property in structure)
        {
          if (property.Key == SerilogProvider.OriginalFormatPropertyName && property.Value is string value)
          {
            messageTemplate = value;
          }
          else if (property.Key.StartsWith("@"))
          {
            if (logger.BindProperty(property.Key.Substring(1), property.Value, true, out var destructured))
              properties.Add(destructured);
          }
          else
          {
            if (logger.BindProperty(property.Key, property.Value, false, out var bound))
              properties.Add(bound);
          }
        }

        var stateType = state.GetType();
        var stateTypeInfo = stateType.GetTypeInfo();
        // Imperfect, but at least eliminates `1 names
        if (messageTemplate == null && !stateTypeInfo.IsGenericType)
        {
          messageTemplate = "{" + stateType.Name + ":l}";
          if (logger.BindProperty(stateType.Name, AsLoggableValue(state, formatter), false, out var stateTypeProperty))
            properties.Add(stateTypeProperty);
        }
      }

      if (messageTemplate == null)
      {
        string propertyName = null;
        if (state != null)
        {
          propertyName = "State";
          messageTemplate = "{State:l}";
        }
        else if (formatter != null)
        {
          propertyName = "Message";
          messageTemplate = "{Message:l}";
        }

        if (propertyName != null)
        {
          if (logger.BindProperty(propertyName, AsLoggableValue(state, formatter), false, out var property))
            properties.Add(property);
        }
      }

      if (eventId.Id != 0 || eventId.Name != null)
        properties.Add(CreateEventIdProperty(eventId));

      var parsedTemplate = _messageTemplateParser.Parse(messageTemplate ?? "");
      var evt = new LogEvent(DateTimeOffset.Now, level, exception, parsedTemplate, properties);
      logger.Write(evt);
    }

    private static object AsLoggableValue<TState>(TState state, Func<TState, Exception, string> formatter)
    {
      object sobj = state;
      if (formatter != null)
        sobj = formatter(state, null);
      return sobj;
    }

    private static LogEventLevel ConvertLevel(LogLevel logLevel)
    {
      switch (logLevel)
      {
        case LogLevel.Critical:
          return LogEventLevel.Fatal;
        case LogLevel.Error:
          return LogEventLevel.Error;
        case LogLevel.Warning:
          return LogEventLevel.Warning;
        case LogLevel.Information:
          return LogEventLevel.Information;
        case LogLevel.Debug:
          return LogEventLevel.Debug;
        // ReSharper disable once RedundantCaseLabel
        case LogLevel.Trace:
        default:
          return LogEventLevel.Verbose;
      }
    }

    private static LogEventProperty CreateEventIdProperty(EventId eventId)
    {
      var properties = new List<LogEventProperty>(2);

      if (eventId.Id != 0)
      {
        properties.Add(new LogEventProperty("Id", new ScalarValue(eventId.Id)));
      }

      if (eventId.Name != null)
      {
        properties.Add(new LogEventProperty("Name", new ScalarValue(eventId.Name)));
      }

      return new LogEventProperty("EventId", new StructureValue(properties));
    }
  }
}
