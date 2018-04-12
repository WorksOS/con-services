using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace VSS.MasterData.Proxies
{
  public static class RequestUtils
  {
    public static IDictionary<string, string> GetCustomHeaders(this IHeaderDictionary headers, bool internalContext=false)
    {
      var customHeaders = new Dictionary<string, string>();

      string[] keys = { "X-VisionLink-CustomerUid" , "X-VisionLink-UserUid", "X-VisionLink-ClearCache", "Authorization", "X-Request-ID", internalContext ? "X-Jwt-Assertion" : "" };
      foreach (var key in keys)
      {
        if (headers.ContainsKey(key))
        {
          customHeaders.Add(key, headers[key]);
        }
      }
      
      return customHeaders;
    }
  }
}
