using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace MasterDataProxies
{
  public static class RequestUtils
  {
    public static IDictionary<string, string> GetCustomHeaders(this IHeaderDictionary headers)
    {
      var customHeaders = new Dictionary<string, string>();

      string[] keys = { "X-VisionLink-CustomerUid" , "X-VisionLink-UserUid", "Authorization" };
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
