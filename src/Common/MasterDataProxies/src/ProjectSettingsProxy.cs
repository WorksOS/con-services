using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Proxies
{
  public class ProjectSettingsProxy : BaseProxy, IProjectSettingsProxy
  {
    public ProjectSettingsProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(configurationStore, logger, cache)
    {
    }

    public async Task<JObject> GetProjectSettings(string projectUid, string userId, IDictionary<string, string> customHeaders)
    {
      var result = await GetMasterDataItem<ProjectSettingsDataResult>(projectUid, userId,
        "PROJECT_SETTINGS_CACHE_LIFE", "PROJECT_SETTINGS_API_URL", customHeaders, $"/projectsettings/{projectUid}");

      if (result.Code == 0)
      {
        return result.Settings;
      }

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

      var result = await GetMasterDataItem<ProjectSettingsDataResult>(projectUid + settingsType, userId,
        "PROJECT_SETTINGS_CACHE_LIFE", "PROJECT_SETTINGS_API_URL", customHeaders, uri);

      if (result.Code == 0)
      {
        return result.Settings;
      }
 
      log.LogWarning("Failed to get project settings, using default values: {0}, {1}", result.Code, result.Message);
      return null;
      
    }

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="projectUid">The projectUid of the item to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    public void ClearCacheItem(string projectUid, string userId)
    {
      ClearCacheItem<ProjectSettingsDataResult>(projectUid, userId);
    }
  }
}
