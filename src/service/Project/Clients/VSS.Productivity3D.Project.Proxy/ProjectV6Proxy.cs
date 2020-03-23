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

    public async Task<List<ProjectData>> GetProjects(string customerUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetMasterDataItemServiceDiscovery<ProjectDataResult>("/project", customerUid, null, customHeaders);

      if (result.Code == 0)
        return result.ProjectDescriptors;

      log.LogDebug($"Failed to get list of projects: {result.Code}, {result.Message}");
      return null;
    }

    // customHeaders will include customerUid
    public async Task<ProjectData> GetProjectForCustomer(string customerUid, string projectUid,
      IDictionary<string, string> customHeaders = null)
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
      IDictionary<string, string> customHeaders = null)
    {
      return await GetItemWithRetry<ProjectDataResult, ProjectData>(GetProjects, p => p.ShortRaptorProjectId == shortRaptorProjectId, customerUid, customHeaders);
    }

    #region applicationContext

    // customHeaders will NOT include customerUid
    public async Task<ProjectData> GetProjectApplicationContext(string projectUid, IDictionary<string, string> customHeaders = null)
    {
      // todoMaverick
      // ProjectSvc.ProjectController should be able to get this from localDB now.
      // response should include customerUid

      var result = await GetMasterDataItemServiceDiscovery<ProjectDataSingleResult>($"/project/applicationcontext/{projectUid}",
             projectUid,
             null,
             customHeaders);

      if (result.Code == 0)
        return result.ProjectDescriptor;

      log.LogDebug($"Failed to get project with Uid {projectUid} for applicationContext: {result.Code}, {result.Message}");
      return null;
    }

    // customHeaders will NOT include customerUid
    public async Task<ProjectData> GetProjectApplicationContext(long shortRaptorProjectId, IDictionary<string, string> customHeaders = null)
    {
      // todoMaverick
      // ProjectSvc.ProjectController should be able to get this from localDB now.
      // response should include customerUid

      var result = await GetMasterDataItemServiceDiscovery<ProjectDataSingleResult>($"/project/applicationcontext/{shortRaptorProjectId}",
             shortRaptorProjectId.ToString(),
             null,
             customHeaders);

      if (result.Code == 0)
        return result.ProjectDescriptor;

      log.LogDebug($"Failed to get project with shortRaptorProjectId {shortRaptorProjectId} for applicationContext: {result.Code}, {result.Message}");
      return null;
    }

    // customHeaders will NOT include customerUid
    public async Task<List<ProjectData>> GetIntersectingProjectsApplicationContext(string customerUid,
        double latitude, double longitude, string projectUid = null, DateTime? timeOfPosition = null,
        IDictionary<string, string> customHeaders = null)
    {
      // todoMaverick
      // ProjectSvc.ProjectController should:
      //  if projectUid, get it if it overlaps in localDB
      //    else get overlapping projects in localDB for this CustomerUID
      //  Note that if timeOfPosition == null, don't check it.

      var topAsString = timeOfPosition == null ? null : timeOfPosition.ToString();
      var queryParameters = new List<KeyValuePair<string, string>>{
          new KeyValuePair<string, string>("customerUid", customerUid),
          new KeyValuePair<string, string>( "latitude",latitude.ToString()),
          new KeyValuePair<string, string>( "longitude",longitude.ToString()),
          new KeyValuePair<string, string>( "timeOfPosition",topAsString) };
      var result = await GetMasterDataItemServiceDiscovery<ProjectDataResult>("/project/applicationcontext/intersecting",
        customerUid, null, customHeaders, queryParameters);

      if (result.Code == 0)
        if (!string.IsNullOrEmpty(projectUid))
          return result.ProjectDescriptors.Where(p => string.Compare(p.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase) == 0).ToList();
        else
          return result.ProjectDescriptors;

      log.LogDebug($"Failed to get list of projects which intersect: {result.Code}, {result.Message}");
      return null;
    }

    // Have device
    //    get list of projects the loc is within (note if manualImport then time will be null, dont' check time)
    //    want to get list of projects it is associated with
    //    want to know if loc within any of these todoMaverick may be quicker to do point-in-poly first, then 
    //    if lo , want to know if loc within it
    //     also, if device is !enpty, then see if device is associated with the project (and claimed etc)

    // todoMaverick not needed anymore?
    //public Task<List<ProjectData>> GetIntersectingProjectsForDeviceApplicationContext(string deviceCustomerUid, string deviceUid,
    //   double latitude, double longitude, DateTime? timeOfPosition = null, IDictionary<string, string> customHeaders = null)
    //{
    //  // todoMaverick ProjectSvc should:
    //  // a) get overlapping project in localDB for this CustomerUID. Note if timeOfPosition = null then don't check time.
    //  // b) if >1 project, and if deviceUid not empty,
    //  //    retrieve list of projects from WM which this device is associated with
    //  //    pair up active projects, good devices and project-device associations
    //  throw new System.NotImplementedException();
    //}

    #endregion applicationContext

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
