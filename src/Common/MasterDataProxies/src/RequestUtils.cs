using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Http;

namespace VSS.MasterData.Proxies
{
  public static class RequestUtils
  {
    /// <summary>
    /// Returns a collection of key value pair VSS custom application headers.
    /// </summary>
    /// <param name="headers">The input header dictionary to match keys from.</param>
    /// <param name="internalContext">As part of the fix for #83476 we now ignore internalContext parameter.</param>
    [Obsolete("Use Strip Headers instead, from the Proxy Level rather than Controller Level")]
    public static IDictionary<string, string> GetCustomHeaders(this IHeaderDictionary headers, bool internalContext = false)
    {
      var customHeaders = new Dictionary<string, string>();

      string[] keys = {
        HeaderConstants.X_VISION_LINK_CUSTOMER_UID,
        HeaderConstants.X_VISION_LINK_USER_UID,
        HeaderConstants.X_VISION_LINK_CLEAR_CACHE,
        HeaderConstants.AUTHORIZATION,
        HeaderConstants.X_REQUEST_ID,
        HeaderConstants.REQUEST_ID,
        HeaderConstants.X_VSS_REQUEST_ID,
        HeaderConstants.X_JWT_ASSERTION
      };

      var prefixList = HeaderConstants.InternalHeaderPrefix;

      foreach (var key in keys)
      {
        if (headers.ContainsKey(key))
        {
          customHeaders.Add(key, headers[key]);
        }
      }

      foreach (var prefix in prefixList)
      {
        var match = headers.FirstOrDefault(h => h.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrEmpty(match.Key) || string.IsNullOrEmpty(match.Value))
          continue;

        customHeaders.Add(match.Key, match.Value);
      }

      return customHeaders;
    }

    public static IDictionary<string, string> AppendOrOverwriteCustomerHeader(this IDictionary<string, string> headers, string customerUid)
    {
      headers[HeaderConstants.X_VISION_LINK_CUSTOMER_UID] = customerUid;
      return headers;
    }

    public static void StripHeaders(this IDictionary<string, string> headers, bool isInternal = true)
    {
      if (headers == null)
      {
        return;
      }

      // Depending of if we are internal, or external, we need different headers to persist or be removed
      var keysToKeep = isInternal
        ? HeaderConstants.InternalHeaders
        : HeaderConstants.ExternalHeaders;

      var prefixList = isInternal
        ? HeaderConstants.InternalHeaderPrefix
        : new List<string>();

      // Have to store the keys here, or else we modify the dictionary while iterating
      var keys = headers.Keys.ToList();

      foreach (var headerKey in keys)
      {
        if (keysToKeep.Any(k => k.Equals(headerKey, StringComparison.OrdinalIgnoreCase)))
          continue;

        if(prefixList.Any(p => headerKey.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
          continue;

        headers.Remove(headerKey);
      }
    }
  }
}
