using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Productivity3D.Project.Proxy
{
  [Obsolete("Use ProjectV4ListServiceDiscoveryProxy instead")]
  public class ProjectProxy : BaseProxy, IProjectProxy
  {
    public ProjectProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache cache) : base(configurationStore, logger, cache)
    {
    }

    public async Task<List<ProjectData>> GetProjectsV4(string customerUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetContainedMasterDataList<ProjectDataResult>(customerUid, null, "PROJECT_CACHE_LIFE", "PROJECT_API_URL", customHeaders);

      if (result.Code == 0)
        return result.ProjectDescriptors;

      log.LogDebug("Failed to get list of projects: {0}, {1}", result.Code, result.Message);
      return null;
    }

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="uid">The uid of the item (either customerUid or projectUid) to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    public void ClearCacheItem(string uid, string userId=null)
    {
      ClearCacheByTag(uid);

      if(string.IsNullOrEmpty(userId))
        ClearCacheByTag(userId);
    }
 
    public async Task<ProjectData> GetProjectForCustomer(string customerUid, string projectUid,
      IDictionary<string, string> customHeaders = null)
    {
      var result = await GetMasterDataItem<ProjectDataSingleResult>(projectUid, 
        null, 
        "PROJECT_CACHE_LIFE",
        "PROJECT_API_URL", 
        customHeaders,
        $"/{projectUid}");

      if (result.Code == 0)
        return result.ProjectDescriptor;

      log.LogDebug("Failed to get project with Uid {0}: {1}, {2}", projectUid, result.Code, result.Message);
      return null;
    }

    //To support 3dpm v1 end points which use legacy project id
    public async Task<ProjectData> GetProjectForCustomer(string customerUid, long projectId,
      IDictionary<string, string> customHeaders = null)
    {
      return await GetItemWithRetry<ProjectDataResult, ProjectData>(GetProjectsV4, p => p.LegacyProjectId == projectId, customerUid, customHeaders);
    }

  }
}
