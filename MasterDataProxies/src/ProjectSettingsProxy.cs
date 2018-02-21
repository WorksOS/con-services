using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Proxies
{
  public class ProjectSettingsProxy : BaseProxy, IProjectSettingsProxy
  {
    public ProjectSettingsProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(configurationStore, logger, cache)
    {
    }

    public async Task<JObject> GetProjectSettings(string projectUid, string userId, IDictionary<string, string> customHeaders, ProjectSettingsType settingsType)
    {
      var result = await GetMasterDataItem<ProjectSettingsDataResult>(projectUid, userId,
        "PROJECT_SETTINGS_CACHE_LIFE", "PROJECT_SETTINGS_API_URL", customHeaders, $"/{projectUid}/{settingsType}");

      if (result.Code == 0)
      {
        return result.Settings;
      }
 
      log.LogWarning("Failed to get project settings {0}, using default values: {1}, {2}", nameof(settingsType), result.Code, result.Message);
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
