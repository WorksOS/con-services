using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace VSS.MasterData.Proxies
{
  public static class LoggingExtensions
  {
    public static string LogHeaders(this IDictionary<string, string> headers) => headers == null ? null : LogHeaders(headers, int.MaxValue);

    public static string LogHeaders(this IDictionary<string, string> headers, int logMaxChar, Formatting formatting = Formatting.None) => headers == null ? null : JsonConvert.SerializeObject(headers, formatting).Truncate(logMaxChar);

    public static string LogHeaders(this IHeaderDictionary headers) => headers == null ? null : LogHeaders(headers, int.MaxValue);

    public static string LogHeaders(this IHeaderDictionary headers, int logMaxChar, Formatting formatting = Formatting.None) => headers == null ? null : JsonConvert.SerializeObject(headers, formatting).Truncate(logMaxChar);
  }
}
