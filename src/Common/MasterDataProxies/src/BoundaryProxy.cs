using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  public class BoundaryProxy : BaseProxy, IBoundaryProxy
  {
    public BoundaryProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(
      configurationStore, logger, cache)
    {
    }


    public async Task<List<GeofenceData>> GetBoundaries(string projectUid,
      IDictionary<string, string> customHeaders = null)
    {
      var result = await GetContainedMasterDataList<GeofenceListData>(projectUid, null, "FILTER_CACHE_LIFE", "FILTER_API_URL",
        customHeaders, $"/{projectUid}", "/boundaries");
      if (result.Code == 0)
      {
        return result.GeofenceData;
      }
      else
      {
        log.LogWarning("Failed to get custom boundaries: {0}, {1}", result.Code, result.Message);
        return null;
      }
    }

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="filterUid">The filterUid of the item to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    public void ClearCacheItem(string filterUid, string userId = null)
    {
      ClearCacheItem<FilterData>(filterUid, userId);
    }

    public async Task<GeofenceData> GetBoundaryForProject(string projectUid, string geofenceUid,
      IDictionary<string, string> customHeaders = null)
    {
      return await GetItemWithRetry<GeofenceListData, GeofenceData>(GetBoundaries, g => string.Equals(g.GeofenceUID.ToString(), geofenceUid, StringComparison.OrdinalIgnoreCase), projectUid, customHeaders);
    }

  }
}
