using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.GenericConfiguration;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies.Models;

namespace VSS.Raptor.Service.Common.Proxies
{
  /// <summary>
  /// Proxy to get geofence data from master data service.
  /// </summary>
  public class GeofenceProxy : BaseProxy<GeofenceData>, IGeofenceProxy
  {
    private static TimeSpan geofenceCacheLife = new TimeSpan(0, 15, 0);//TODO: how long to cache ?

    public GeofenceProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(configurationStore, logger, cache)
    {
    }

    /// <summary>
    /// Gets the geofence boundary in WKT format for a given UID.
    /// </summary>
    /// <param name="geofenceUid">The geofence UID</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
    /// <returns></returns>
    public string GetGeofenceBoundary(string geofenceUid, IDictionary<string, string> customHeaders = null)
    {
      GeofenceData cacheData = GetItem(geofenceUid, geofenceCacheLife, "GEOFENCE_API_URL", customHeaders);
      return cacheData == null ? null : cacheData.GeometryWKT;
    }
  }
}

