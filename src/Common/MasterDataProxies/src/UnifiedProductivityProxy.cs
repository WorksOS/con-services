using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  public class UnifiedProductivityProxy : BaseProxy, IUnifiedProductivityProxy
  {
    public UnifiedProductivityProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache cache) : base(
      configurationStore, logger, cache)
    {
    }

    /// <summary>
    /// Gets the list of geofences associated with the project
    /// </summary>
    public async Task<List<GeofenceData>> GetAssociatedGeofences(string projectUid, IDictionary<string, string> customHeaders = null)
    {
      var route = $"/projects/{projectUid}/sitewithtargets/asgeofence";
      var result = await GetContainedMasterDataList<GeofenceWithTargetsResult>(projectUid, null, "UNIFIED_PRODUCTIVITY_CACHE_LIFE", "UNIFIED_PRODUCTIVITY_API_URL", customHeaders, null, route);
      return result.Results?.Select(r => r.Geofence).ToList();
    }

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    public void ClearCacheItem(string projectUid, string userId = null)
    {
      ClearCacheItem<GeofenceWithTargetsResult>(projectUid, userId);
    }

  }
}
