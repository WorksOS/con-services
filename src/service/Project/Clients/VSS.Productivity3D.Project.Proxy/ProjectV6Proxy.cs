using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.Project.Proxy
{
  public class ProjectV6Proxy : BaseServiceDiscoveryProxy, IProjectProxy
  {
    public ProjectV6Proxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.Project;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V6;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "PROJECT_CACHE_LIFE";

    public async Task<List<ProjectData>> GetProjects(string customerUid, IHeaderDictionary customHeaders = null)
    {
      var result = await GetMasterDataItemServiceDiscovery<ProjectDataResult>("/project", customerUid, null, customHeaders);

      if (result.Code == 0)
        return result.ProjectDescriptors;

      log.LogDebug($"Failed to get list of projects: {result.Code}, {result.Message}");
      return null;
    }

    // customHeaders will include customerUid
    public async Task<ProjectData> GetProjectForCustomer(string customerUid, string projectUid,
      IHeaderDictionary customHeaders = null)
    {
      var result = await GetMasterDataItemServiceDiscovery<ProjectDataSingleResult>($"/project/{projectUid}",
        projectUid,
        null,
        customHeaders);

      if (result.Code == 0)
        return result.ProjectDescriptor;

      log.LogDebug($"Failed to get project with Uid {projectUid} for customer: {customerUid}: {result.Code}, {result.Message}");
      return null;
    }


    //To support 3dpm v1 end points which use legacy project id
    public async Task<ProjectData> GetProjectForCustomer(string customerUid, long shortRaptorProjectId,
      IHeaderDictionary customHeaders = null)
    {
      return await GetItemWithRetry<ProjectDataResult, ProjectData>(GetProjects, p => p.ShortRaptorProjectId == shortRaptorProjectId, customerUid, customHeaders);
    }

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="uid">The uid of the item (either customerUid or projectUid) to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    public void ClearCacheItem(string uid, string userId = null)
    {
      ClearCacheByTag(uid);

      if (string.IsNullOrEmpty(userId))
        ClearCacheByTag(userId);
    }
  }
}
