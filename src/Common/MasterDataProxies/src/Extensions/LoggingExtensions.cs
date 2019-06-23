using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.MasterData.Proxies
{
  public static class LoggingExtensions
  {
    public static string LogHeaders(this IDictionary<string, string> headers)
    {
      return headers == null ? null : LogHeaders(headers, int.MaxValue);
    }

    public static string LogHeaders(this IDictionary<string, string> headers, int logMaxChar, Formatting formatting = Formatting.None)
    {
      return headers == null ? null : JsonConvert.SerializeObject(headers, formatting).Truncate(logMaxChar);
    }
  }
}
