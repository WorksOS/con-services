using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Http;

namespace VSS.MasterData.Proxies
{
  public static class RequestUtils
  {
    public static IHeaderDictionary AppendOrOverwriteCustomerHeader(this IHeaderDictionary headers, string customerUid)
    {
      headers[HeaderConstants.X_VISION_LINK_CUSTOMER_UID] = customerUid;
      return headers;
    }

    public static IHeaderDictionary GetCustomHeaders(this IHeaderDictionary headers)
    {
      var customHeaders = new HeaderDictionary();

      if (headers != null)
      {
        foreach (var headerKey in headers)
        {
          if (HeaderConstants.InternalHeaders.Any(k => k.Contains(headerKey.Key, StringComparison.OrdinalIgnoreCase)))
          {
            customHeaders.Add(headerKey);
          }

          if (HeaderConstants.InternalHeaderPrefix.Any(p => headerKey.Key.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
          {
            customHeaders.Add(headerKey);
          }
        }
      }

      return customHeaders;
    }

    public static void StripHeaders(this IHeaderDictionary headers, bool isInternal = true)
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

        if (prefixList.Any(p => headerKey.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
          continue;

        headers.Remove(headerKey);
      }
    }
  }
}
