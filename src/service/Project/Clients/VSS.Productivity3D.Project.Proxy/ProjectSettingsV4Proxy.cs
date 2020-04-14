using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Project.Proxy
{
  public class ProjectSettingsV4Proxy : BaseServiceDiscoveryProxy, IProjectSettingsProxy
  {
    public ProjectSettingsV4Proxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.Project;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V4;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "PROJECT_SETTINGS_CACHE_LIFE";

    public async Task<JObject> GetProjectSettings(string projectUid, string userId, IDictionary<string, string> customHeaders)
    {
      var result = await GetMasterDataItemServiceDiscovery<ProjectSettingsDataResult> ($"/projectsettings/{projectUid}", projectUid, userId, customHeaders );

      if (result.Code == 0)
        return result.Settings;

      log.LogWarning($"Failed to get project settings, using default values: {result.Code}, {result.Message}");
      return null;
    }

    public async Task<JObject> GetProjectSettings(string projectUid, string userId, IDictionary<string, string> customHeaders, ProjectSettingsType settingsType)
    {
      var uri = string.Empty;
      switch (settingsType)
      {
        case ProjectSettingsType.Targets:
          uri = $"/projectsettings/{projectUid}";
          break;
        case ProjectSettingsType.Colors:
          uri = $"/projectcolors/{projectUid}";
          break;
          default:
          throw new ServiceException(HttpStatusCode.BadRequest,new ContractExecutionResult(-10,"Unsupported project settings type."));
      }

      var result = await GetMasterDataItemServiceDiscovery<ProjectSettingsDataResult> (uri,projectUid + settingsType, userId, customHeaders );

      if (result.Code == 0)
        return result.Settings;
 
      log.LogWarning($"Failed to get project settings by type {settingsType.ToString()}, using default values: {result.Code}, {result.Message}");
      return null;
      
    }

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="projectUid">The projectUid of the item to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    public void ClearCacheItem(string projectUid, string userId)
    {
      ClearCacheByTag(projectUid);
      ClearCacheByTag(userId);
    }
  }
}
