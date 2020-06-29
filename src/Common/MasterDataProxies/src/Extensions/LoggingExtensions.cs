using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace VSS.MasterData.Proxies
{
  public static class LoggingExtensions
  {
    public static string LogHeaders(this IDictionary<string, string> headers) => headers == null ? null : LogHeaders(headers, int.MaxValue);

    public static string LogHeaders(this IDictionary<string, string> headers, int logMaxChar, Formatting formatting = Formatting.None) => headers == null ? null : JsonConvert.SerializeObject(headers, formatting).Truncate(logMaxChar);

    public static string LogHeaders(this IHeaderDictionary headers) => headers == null ? null : LogHeaders(headers, int.MaxValue);

    public static string LogHeaders(this IHeaderDictionary headers, int logMaxChar, Formatting formatting = Formatting.None) => LogHeadersInSizeOrder(headers, logMaxChar, formatting);

    private static string LogHeadersInSizeOrder(IDictionary<string, string> headers, int logMaxChar, Formatting formatting)
    {
      if (headers == null)
        return null;
      var orderedHeaders = headers.OrderBy(h => h.Value.Length);
      return JsonConvert.SerializeObject(orderedHeaders, formatting).Truncate(logMaxChar);
    }

    private static string LogHeadersInSizeOrder(IHeaderDictionary headers, int logMaxChar, Formatting formatting)
    {
      if (headers == null)
        return null;
      var orderedHeaders = headers.OrderBy(h => h.Value[0].Length);
      return JsonConvert.SerializeObject(orderedHeaders, formatting).Truncate(logMaxChar);
    }
  }
}
