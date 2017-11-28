using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace VSS.MasterData.Proxies
{
  public static class RequestUtils
  {
    public static IDictionary<string, string> GetCustomHeaders(this IHeaderDictionary headers)
    {
      var customHeaders = new Dictionary<string, string>();

      string[] keys = { "X-VisionLink-CustomerUid" , "X-VisionLink-UserUid", "Authorization", "X-VisionLink-ClearCache" };
      foreach (var key in keys)
      {
        if (headers.ContainsKey(key))
        {
          customHeaders.Add(key, headers[key]);
        }
      }
      
      return customHeaders;
    }

    public static IDictionary<string, string> GetCustomHeadersInternalContext(this IHeaderDictionary headers)
    {
      var customHeaders = new Dictionary<string, string>();

      string[] keys = { "X-VisionLink-CustomerUid", "X-VisionLink-UserUid", "X-Jwt-Assertion", "X-VisionLink-ClearCache" };
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
