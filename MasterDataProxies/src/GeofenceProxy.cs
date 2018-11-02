using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories.DBModels;

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
      //Get all types of geofence except Project=1 and Import=8/Export=9 (and 3dpm boundaries which are type Filter=11)
      List<int> geofenceTypeIds = new List<int>
        { (int)GeofenceType.Generic, (int)GeofenceType.Borrow, (int)GeofenceType.Waste, (int)GeofenceType.AvoidanceZone,
          (int)GeofenceType.Stockpile, (int)GeofenceType.CutZone, (int)GeofenceType.FillZone, (int)GeofenceType.Landfill
        };
      var queryParams = $"?geofenceTypeIds={string.Join("&geofenceTypeIds=", geofenceTypeIds)}";
      var result = await GetContainedMasterDataList<GeofenceDataResult>(customerUid, null, "GEOFENCE_CACHE_LIFE", "GEOFENCE_API_URL", customHeaders, queryParams);
      return result.Geofences;
    }

    public async Task<Guid> CreateGeofence(Guid customerGuid, string geofenceName, string description,
        string geofenceType, string geometryWKT, int fillColor, bool isTransparent, Guid userUid, double areaSqMeters,
        IDictionary<string, string> customHeaders = null)
    {
      var geofenceGuid = Guid.NewGuid();

      // as of this writing, GeofenceSvc ignores this geofenceGuid for User-context, but not application-context
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

      var result = await SendRequest<GeofenceCreateResult>("GEOFENCE_API_URL", JsonConvert.SerializeObject(payLoadToSend),
        customHeaders, String.Empty, "POST", String.Empty);

      ClearCacheItem<GeofenceDataResult>(customerGuid.ToString(), userUid.ToString());

      //// potentally another user could have created a geofence, so
      ////    can'y rely on just retrieving one extra Geofence, and it being our new one.
      ////    Ensure the comparer in  GeofenceData only includes the fields we want e.g. NOT GeofenceUid
      //var updatedGeofences = await GetGeofences(customerGuid.ToString(), customHeaders);
      //var geofence = updatedGeofences.FirstOrDefault(g => g.GeofenceUID == geofenceGuid || Equals(g, payLoadToSend));

      log.LogInformation($"GeofenceProxy.CreateGeofence. payloadToSend: {JsonConvert.SerializeObject(payLoadToSend)} result: {JsonConvert.SerializeObject(result)}");
      var guidToReturn = Guid.Empty;
      if (result != null)
      {
        guidToReturn = Guid.Parse(result.geofenceUID);
      }
      
      return guidToReturn;
    }

    public async Task<Guid> UpdateGeofence(Guid geofenceGuid, Guid customerGuid, string geofenceName, string description,
      string geofenceType, string geometryWKT, int fillColor, bool isTransparent, Guid userUid, double areaSqMeters,
      IDictionary<string, string> customHeaders = null)
    {
      var payLoadToSend = new GeofenceDataForUpdate()
      {
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
      await SendRequest<OkResult>("GEOFENCE_API_URL", JsonConvert.SerializeObject(payLoadToSend),
        customHeaders, String.Empty, "PUT", String.Empty);

      ClearCacheItem<GeofenceDataResult>(customerGuid.ToString(), userUid.ToString());

      return geofenceGuid;
    }

   

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="customerUid">The customerUid of the items to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    public void ClearCacheItem(string customerUid, string userId = null)
    {
      ClearCacheItem<GeofenceDataResult>(customerUid, userId);
    }

    public async Task<GeofenceData> GetGeofenceForCustomer(string customerUid, string geofenceUid,
      IDictionary<string, string> customHeaders = null)
    {
      return await GetItemWithRetry<GeofenceDataResult, GeofenceData>(GetGeofences, g => string.Equals(g.GeofenceUID.ToString(), geofenceUid, StringComparison.OrdinalIgnoreCase), customerUid, customHeaders);
    }
  }
}

