using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
  /// <summary>
  /// These are called internally from TFA
  /// </summary>
  public class ProjectInternalV6Proxy : BaseServiceDiscoveryProxy, IProjectInternalProxy
  {
    public ProjectInternalV6Proxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.Project;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V6;

    public override ApiType Type => ApiType.Private;

    public override string CacheLifeKey => "PROJECT_INTERNAL_CACHE_LIFE";

    public async Task<List<ProjectData>> GetProjects(string customerUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetMasterDataItemServiceDiscovery<ProjectDataResult>($"/project/{customerUid}/projects", customerUid, null, customHeaders);

      if (result.Code == 0)
        return result.ProjectDescriptors;

      log.LogDebug($"Failed to get list of projects: {result.Code}, {result.Message}");
      return null;
    }

    public async Task<ProjectData> GetProject(string projectUid, IDictionary<string, string> customHeaders = null)
    {
      // ProjectSvc.ProjectController get this from localDB now.
      // response includes customerUid

      var result = await GetMasterDataItemServiceDiscovery<ProjectDataSingleResult>($"/project/{projectUid}",
             projectUid,
             null,
             customHeaders);

      if (result.Code == 0 && result.ProjectDescriptor != null)
        return result.ProjectDescriptor;

      log.LogDebug($"Failed to get project with Uid {projectUid} for applicationContext: {result.Code}, {result.Message}");
      return null;
    }

    /// <summary>
    /// Called from TFA
    ///    application token i.e. customHeaders will NOT include customerUid
    ///    ProjectSvc.ProjectController should be able to get this from localDB now.
    ///       response to include customerUid
    /// </summary>
    public async Task<ProjectData> GetProject(long shortRaptorProjectId, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetMasterDataItemServiceDiscovery<ProjectDataSingleResult>($"/project/shortId/{shortRaptorProjectId}",
             shortRaptorProjectId.ToString(),
             null,
             customHeaders);

      if (result.Code == 0)
        return result.ProjectDescriptor;

      log.LogDebug($"Failed to get project with shortRaptorProjectId {shortRaptorProjectId} for applicationContext: {result.Code}, {result.Message}");
      return null;
    }

    public async Task<List<ProjectData>> GetIntersectingProjects(string customerUid,
        double latitude, double longitude, string projectUid = null, IDictionary<string, string> customHeaders = null)
    {
      // ProjectSvc.ProjectController should:
      //  if projectUid, get it if it overlaps in localDB
      //    else get overlapping projects in localDB for this CustomerUID
            
      var queryParameters = new List<KeyValuePair<string, string>>{
          new KeyValuePair<string, string>("customerUid", customerUid),
          new KeyValuePair<string, string>( "latitude",latitude.ToString()),
          new KeyValuePair<string, string>( "longitude",longitude.ToString()),
          new KeyValuePair<string, string>( "projectUid",projectUid) };
      var result = await GetMasterDataItemServiceDiscovery<ProjectDataResult>("/project/intersecting",
        customerUid, null, customHeaders, queryParameters);

      if (result.Code == 0)
        if (!string.IsNullOrEmpty(projectUid))
          return result.ProjectDescriptors.Where(p => string.Compare(p.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase) == 0).ToList();
        else
          return result.ProjectDescriptors;

      log.LogDebug($"Failed to get list of projects which intersect: {result.Code}, {result.Message}");
      return null;
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
