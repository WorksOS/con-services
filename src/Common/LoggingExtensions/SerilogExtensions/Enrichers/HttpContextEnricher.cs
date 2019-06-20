using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace VSS.Serilog.Extensions.Enrichers
{
  /// <summary>
  /// Custom Serilog enricher that allows us to dig into the IHttpContextAccess::HttpContext object
  /// for the RequestID used to track requests across micro services.
  /// </summary>
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
