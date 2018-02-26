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
  public class FilterServiceProxy : BaseProxy, IFilterServiceProxy
  {
    public FilterServiceProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(
      configurationStore, logger, cache)
    {
    }

    /// <summary>
    /// Gets the filter from the filter service.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <param name="filterUid">The filter uid.</param>
    /// <param name="customHeaders">The custom headers including JWT and customer context.</param>
    /// <returns></returns>
    public async Task<FilterDescriptor> GetFilter(string projectUid, string filterUid,
      IDictionary<string, string> customHeaders = null)
    {
      var result = await GetContainedMasterDataList<FilterData>(filterUid, null, "FILTER_CACHE_LIFE", "FILTER_API_URL",
        customHeaders, $"/{projectUid}?filterUid={filterUid}", "/filter");
      if (result.Code == 0)
      {
        return result.filterDescriptor;
      }
      else
      {
        log.LogWarning("Failed to get Filter Descriptor: {0}, {1}", result.Code, result.Message);
        return null;
      }

    }

    /// <summary>
    /// Gets the filters from the filter service.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <param name="customHeaders">The custom headers including JWT and customer context.</param>
    /// <returns></returns>
    public async Task<List<FilterDescriptor>> GetFilters(string projectUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetContainedMasterDataList<FilterListData>(projectUid, null, "FILTER_CACHE_LIFE", "FILTER_API_URL",
        customHeaders, $"/{projectUid}", "/filters");
      if (result.Code == 0)
      {
        return result.filterDescriptors;
      }
      else
      {
        log.LogWarning("Failed to get Filter Descriptors: {0}, {1}", result.Code, result.Message);
        return null;
      }

    }


    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="filterUid">The filterUid of the item to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    public void ClearCacheItem(string filterUid, string userId=null)
    {
      ClearCacheItem<FilterData>(filterUid, userId);
    }

    /// <summary>
    /// Clears an item containg a list from the cache
    /// </summary>
    /// <param name="projectUid">The projectUid of the item to remove from the cache</param>
    /// <param name="userId">The user ID, only required if caching per user</param>
    public void ClearCacheListItem(string projectUid, string userId=null)
    {
      ClearCacheItem<FilterListData>(projectUid, userId);
    }
  }
}
