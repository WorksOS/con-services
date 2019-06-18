using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace VSS.Serilog.Extensions.Enrichers
{
  /// <summary>
  /// Custom Serilog enricher to parse out line feeds from exception stack messages.
  /// This is so the console output plays nice with FluentD/Kabana and we get only one 'entry' for the exception
  /// instead of one per line.
  /// </summary>
  /// <remarks>
  /// Replaces the default {Exception} enricher.
  /// </remarks>
  public class HttpContextEnricher : ILogEventEnricher
  {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextEnricher(IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor;
    } 

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
      if (_httpContextAccessor?.HttpContext?.Items != null)
      {
        if (_httpContextAccessor.HttpContext.Items.ContainsKey("RequestID"))
        {
          var requestId = " req:" + _httpContextAccessor.HttpContext.Items["RequestID"];

          var logEventProperty = propertyFactory.CreateProperty("RequestID", requestId);
          logEvent.AddPropertyIfAbsent(logEventProperty);
        }
      }
    }
  }
}
