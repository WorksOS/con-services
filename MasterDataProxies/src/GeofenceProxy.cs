using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterDataProxies.Interfaces;

namespace VSS.MasterDataProxies
{
  /// <summary>
  /// Proxy to get geofence data from master data service.
  /// </summary>
  public class GeofenceProxy : BaseProxy, IGeofenceProxy
    {
        private static readonly TimeSpan geofenceCacheLife = new TimeSpan(0, 15, 0); //TODO: how long to cache ?

        public GeofenceProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(
            configurationStore, logger, cache)
        {
        }

        /// <summary>
        /// Gets the geofence boundary in WKT format for a given UID.
        /// </summary>
        /// <param name="geofenceUid">The geofence UID</param>
        /// <param name="customHeaders">The custom headers for the request (authorization, userUid and customerUid)</param>
        /// <returns></returns>
        public async Task<string> GetGeofenceBoundary(string geofenceUid,
            IDictionary<string, string> customHeaders = null)
        {
            GeofenceData cacheData =
                await GetItem<GeofenceData>(geofenceUid, geofenceCacheLife, "GEOFENCE_API_URL", customHeaders);
            return cacheData?.GeometryWKT;
        }

        public async Task<Guid> CreateGeofence(Guid customerGuid, string geofenceName, string description,
            string geofenceType, string geometryWKT, int fillColor, bool isTransparent, Guid userUid,
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
                UserUID = userUid
            };
            await SendRequest<GeofenceData>("CREATEGEOFENCE_API_URL", JsonConvert.SerializeObject(payLoadToSend),
                customHeaders);
            return geofenceGuid;
        }
    }
}

