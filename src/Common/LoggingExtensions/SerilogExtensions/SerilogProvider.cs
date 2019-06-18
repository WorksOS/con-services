using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using FrameworkLogger = Microsoft.Extensions.Logging.ILogger;
using ILogger = Serilog.ILogger;

namespace VSS.Serilog.Extensions
{
  /// <summary>
  /// Custom Serilog provider. Ordinarily we might use serilog.extensions.logging and use the SerilogLoggerProvider
  /// but our dependency on IHttpContextAccessor for request ID information means we need to extend it,
  /// and creating a new implementation is easier than trying to work over the top of serilog.extensions.logging package.
  /// </summary>
  public class SerilogProvider : ILoggerProvider, ILogEventEnricher
  {
    internal const string OriginalFormatPropertyName = "{OriginalFormat}";
    internal const string ScopePropertyName = "Scope";

    private readonly Action _dispose;
    private readonly IHttpContextAccessor _accessor;
    
    private readonly ILogger _logger; // May be null; if it is, Log.Logger will be lazily used

    public SerilogProvider(ILogger logger, IHttpContextAccessor accessor, bool dispose = false)
    {
      if (logger != null)
        _logger = logger.ForContext(new[] { this });

      if (dispose)
      {
        if (logger != null)
          _dispose = () => (logger as IDisposable)?.Dispose();
        else
          _dispose = Log.CloseAndFlush;
      }

      //_accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
    }

    /// <inheritdoc />
    public FrameworkLogger CreateLogger(string name)
    {
      return new SerilogLogger(this, _logger, name);
    }

    /// <inheritdoc cref="IDisposable" />
    public IDisposable BeginScope<T>(T state)
    {
      if (CurrentScope != null)
        return new SerilogLoggerScope(this, state);

      // The outermost scope pushes and pops the Serilog `LogContext` - once
      // this enricher is on the stack, the `CurrentScope` property takes care
      // of the rest of the `BeginScope()` stack.
      var popSerilogContext = LogContext.Push(this);
      return new SerilogLoggerScope(this, state, popSerilogContext);
    }

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
      List<LogEventPropertyValue> scopeItems = null;
      for (var scope = CurrentScope; scope != null; scope = scope.Parent)
      {
        scope.EnrichAndCreateScopeItem(logEvent, propertyFactory, out var scopeItem);

        if (scopeItem != null)
        {
          scopeItems = scopeItems ?? new List<LogEventPropertyValue>();
          scopeItems.Add(scopeItem);
        }
      }

      if (scopeItems != null)
      {
        scopeItems.Reverse();
        logEvent.AddPropertyIfAbsent(new LogEventProperty(ScopePropertyName, new SequenceValue(scopeItems)));
      }
    }

    readonly AsyncLocal<SerilogLoggerScope> _value = new AsyncLocal<SerilogLoggerScope>();

    internal SerilogLoggerScope CurrentScope
    {
      get => _value.Value;
      set => _value.Value = value;
    }

    /// <inheritdoc />
    public void Dispose()
    {
      _dispose?.Invoke();
    }
  }
}
