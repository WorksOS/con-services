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
    public static IDictionary<string, string> GetCustomHeaders(this IHeaderDictionary headers)
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
      // Depending of if we are internal, or external, we need different headers to persist or be removed
      var keysToKeep = isInternal
        ? HeaderConstants.InternalHeaders
        : HeaderConstants.ExternalHeaders;

      // Have to store the keys here, or else we modify the dictionary while iterating
      var keys = headers?.Keys.ToList() ?? new List<string>();

      foreach (var headerKey in keys)
      {
        if (keysToKeep.Any(k => k.Equals(headerKey, StringComparison.OrdinalIgnoreCase)))
          continue;

        headers.Remove(headerKey);
      }
    }

  }
}
