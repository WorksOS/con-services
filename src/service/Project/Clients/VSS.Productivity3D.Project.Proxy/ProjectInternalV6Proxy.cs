using System.Collections.Generic;
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

   public async Task<ProjectData> GetProject(string projectUid, IHeaderDictionary customHeaders = null)
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
   /// ProjectInternalController gets a list of projects for customer and returns intersecting depending on current rules
   /// </summary>
   public async Task<ProjectDataResult> GetIntersectingProjects(string customerUid,
        double latitude, double longitude, string projectUid = null, double? northing = null, double? easting = null, IHeaderDictionary customHeaders = null)
    {
      var queryParameters = new List<KeyValuePair<string, string>>{
          new KeyValuePair<string, string>("customerUid", customerUid),
          new KeyValuePair<string, string>( "latitude",latitude.ToString()),
          new KeyValuePair<string, string>( "longitude",longitude.ToString()),
          new KeyValuePair<string, string>( "projectUid",projectUid),
          new KeyValuePair<string, string>( "northing", northing?.ToString()),
          new KeyValuePair<string, string>( "easting",easting?.ToString()) };
      var result = await GetMasterDataItemServiceDiscovery<ProjectDataResult>("/project/intersecting",
        customerUid, null, customHeaders, queryParameters);

      log.LogDebug($"{nameof(GetIntersectingProjects)} get list of projects which intersect: {result.Code}, {result.Message}");
      return result;
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
