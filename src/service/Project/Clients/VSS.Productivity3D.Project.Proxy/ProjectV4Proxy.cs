using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.Project.Proxy
{
  public class ProjectV4Proxy : BaseServiceDiscoveryProxy, IProjectProxy
  {
    public ProjectV4Proxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution) 
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public  override bool IsInsideAuthBoundary => true;

    public  override ApiService InternalServiceType => ApiService.Project;

    public override string ExternalServiceName => null;

    public  override ApiVersion Version => ApiVersion.V4;

    public  override ApiType Type => ApiType.Public;

    public  override string CacheLifeKey => "PROJECT_CACHE_LIFE";

    public async Task<List<ProjectData>> GetProjectsV4(string customerUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetMasterDataItemServiceDiscovery<ProjectDataResult>(null, customerUid, null, customHeaders);
      
      if (result.Code == 0)
        return result.ProjectDescriptors;

      log.LogDebug($"Failed to get list of projects: {result.Code}, {result.Message}");
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
      var result = await GetMasterDataItemServiceDiscovery<ProjectDataSingleResult>($"/project/{projectUid}",
        projectUid,
        null, 
        customHeaders);

      if (result.Code == 0)
        return result.ProjectDescriptor;

      log.LogDebug($"Failed to get project with Uid {projectUid}: {result.Code}, {result.Message}");
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
