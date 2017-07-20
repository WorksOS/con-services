using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  public class ProjectSettingsProxy : BaseProxy, IProjectSettingsProxy
  {
    public ProjectSettingsProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(configurationStore, logger, cache)
    {
    }

    public async Task<string> GetProjectSettings(string projectUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetItem<ProjectSettingsDataResult>(projectUid, "PROJECT_SETTINGS_CACHE_LIFE", "PROJECT_SETTINGS_API_URL", customHeaders, $"/{projectUid}");

      if (result.Code == 0)
      {
        return result.Settings;
      }
      else
      {
        log.LogDebug("Failed to get project settings: {0}, {1}", result.Code, result.Message);
        return null;
      }
    }
  }
}
