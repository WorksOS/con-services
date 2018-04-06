using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  /// <summary>
  /// Proxy to get geofence data from master data service.
  /// </summary>
  public class GeofenceProxy : BaseProxy, IGeofenceProxy
  {
    public GeofenceProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(
        configurationStore, logger, cache)
    {
    }

    /// <summary>
    /// Gets the geofence boundary in WKT format for a given UID.
    /// </summary>
    /// <param name="customerUid">The customer UID</param>
    /// <param name="geofenceUid">The geofence UID</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
    /// <returns></returns>
    public async Task<string> GetGeofenceBoundary(string customerUid, string geofenceUid,
        IDictionary<string, string> customHeaders = null)
    {
      string geometryWKT = null;
      var geofences = await GetGeofences(customerUid, customHeaders);
      if (geofences != null)
      {
        var geofence = geofences.SingleOrDefault(g => g.GeofenceUID.ToString() == geofenceUid);
        geometryWKT = geofence?.GeometryWKT;

      }
      return geometryWKT;
    }

    /// <summary>
    /// Gets the list of geofences for the customer
    /// </summary>
    /// <param name="customerUid">The customer UID</param>
    /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
    /// <returns></returns>
    public async Task<List<GeofenceData>> GetGeofences(string customerUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetContainedMasterDataList<GeofenceDataResult>(customerUid, null, "GEOFENCE_CACHE_LIFE", "GEOFENCE_API_URL", customHeaders);
      return result.Geofences;
    }

    public async Task<Guid> CreateGeofence(Guid customerGuid, string geofenceName, string description,
        string geofenceType, string geometryWKT, int fillColor, bool isTransparent, Guid userUid, double areaSqMeters,
        IDictionary<string, string> customHeaders = null)
    {
        var geofenceGuid = Guid.NewGuid();
        var payLoadToSend = new GeofenceData()
        {
            CustomerUID = customerGuid,
            GeofenceName = geofenceName,
            Description = description,
            GeofenceType = geofenceType,
            GeometryWKT = geometryWKT,
            FillColor = fillColor,
            IsTransparent = isTransparent,
            GeofenceUID = geofenceGuid,
            UserUID = userUid,
            AreaSqMeters = areaSqMeters
        };
        await SendRequest<GeofenceData>("CREATEGEOFENCE_API_URL", JsonConvert.SerializeObject(payLoadToSend),
            customHeaders,String.Empty, "POST", String.Empty);
        return geofenceGuid;
    }

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="geofenceUid">The geofenceUid of the item to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    public void ClearCacheItem(string geofenceUid, string userId=null)
    {
      ClearCacheItem<GeofenceData>(geofenceUid, userId);
    }

    public async Task<GeofenceData> GetGeofenceForCustomer(string customerUid, string geofenceUid,
      IDictionary<string, string> customHeaders = null)
    {
      return await GetItemWithRetry<GeofenceDataResult, GeofenceData>(GetGeofences, g => string.Equals(g.GeofenceUID.ToString(), geofenceUid, StringComparison.OrdinalIgnoreCase), customerUid, customHeaders);
    }
  }
}

