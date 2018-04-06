using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  public class ProjectListProxy : BaseProxy, IProjectListProxy
  {
    public ProjectListProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(configurationStore, logger, cache)
    {
    }

    public async Task<List<ProjectData>> GetProjectsV4(string customerUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetContainedMasterDataList<ProjectDataResult>(customerUid, null, "PROJECT_CACHE_LIFE", "PROJECT_API_URL", customHeaders);
      if (result.Code == 0)
      {
        return result.ProjectDescriptors;
      }
      else
      {
        log.LogDebug("Failed to get list of projects: {0}, {1}", result.Code, result.Message);
        return null;
      }
    }

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="customerUid">The customerUid of the item to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    public void ClearCacheItem(string customerUid, string userId=null)
    {
      ClearCacheItem<ProjectDataResult>(customerUid, userId);
    }
 
    public async Task<ProjectData> GetProjectForCustomer(string customerUid, string projectUid,
      IDictionary<string, string> customHeaders = null)
    {
      return await GetItemWithRetry<ProjectDataResult, ProjectData>(GetProjectsV4, p => string.Equals(p.ProjectUid, projectUid, StringComparison.OrdinalIgnoreCase), customerUid, customHeaders);
    }

  }
}
