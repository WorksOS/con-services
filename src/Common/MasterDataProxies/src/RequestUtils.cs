using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Http;

namespace VSS.MasterData.Proxies
{
  public static class RequestUtils
  {
    [Obsolete("Use Strip Headers instead, from the Proxy Level rather than Controller Level")]
    public static IDictionary<string, string> GetCustomHeaders(this IHeaderDictionary headers, bool internalContext=false)
    {
      var customHeaders = new Dictionary<string, string>();

      string[] keys = { 
        HeaderConstants.X_VISION_LINK_CUSTOMER_UID , 
        HeaderConstants.X_VISION_LINK_USER_UID, 
        HeaderConstants.X_VISION_LINK_CLEAR_CACHE, 
        HeaderConstants.AUTHORIZATION, 
        HeaderConstants.X_REQUEST_ID, 
        HeaderConstants.REQUEST_ID, 
        HeaderConstants.X_VSS_REQUEST_ID, 
        internalContext ? HeaderConstants.X_JWT_ASSERTION : string.Empty
      };

      foreach (var key in keys)
      {
        if (headers.ContainsKey(key))
        {
          customHeaders.Add(key, headers[key]);
        }
      }
      
      return customHeaders;
    }

    public static void StripHeaders(this IDictionary<string, string> headers, bool isInternal)
    {
      var keys = new List<string>
      {
        HeaderConstants.X_VISION_LINK_CUSTOMER_UID , 
        HeaderConstants.X_VISION_LINK_USER_UID, 
        HeaderConstants.X_VISION_LINK_CLEAR_CACHE, 
        HeaderConstants.AUTHORIZATION, 
        HeaderConstants.X_REQUEST_ID, 
        HeaderConstants.REQUEST_ID, 
        HeaderConstants.X_VSS_REQUEST_ID, 
      };
      if(isInternal)
        keys.Add(HeaderConstants.X_JWT_ASSERTION);

      var dictKeys = headers.Keys.ToList();

      foreach (var dictKey in dictKeys)
      {
        if(keys.Any(k => k.Equals(dictKey, StringComparison.OrdinalIgnoreCase)))
          continue;

        headers.Remove(dictKey);
      }
    }
  }
}
