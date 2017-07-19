using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  public class ProjectProxy : BaseProxy, IProjectProxy
  {
    private static TimeSpan projectListCacheLife = new TimeSpan(0, 15, 0);//TODO: how long to cache ?
    private static TimeSpan fileListCacheLife = new TimeSpan(0, 15, 0);//TODO: how long to cache ?
    private static TimeSpan projectSettingsCacheLife = new TimeSpan(0, 15, 0);//TODO: how long to cache ?

    public ProjectProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(configurationStore, logger, cache)
    {
    }

    public async Task<List<ProjectData>> GetProjectsV4(string customerUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetContainedList<ProjectDataResult>(customerUid, projectListCacheLife, "PROJECT_API_URL", customHeaders);
      if (result.Code == 0)
      {
        return result.ProjectDescriptors;
      }
      else
      {
        log.LogDebug("Failed to get list of projects: {0}, {1}", result.Code, result.Message);
        return null;
      }
    }

    public async Task<List<FileData>> GetFiles(string projectUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetContainedList<FileDataResult>(projectUid, fileListCacheLife, "IMPORTED_FILE_API_URL", customHeaders,
        string.Format("?projectUid={0}", projectUid));
      if (result.Code == 0)
      {
        return result.ImportedFileDescriptors;
      }
      else
      {
        log.LogDebug("Failed to get list of files: {0}, {1}", result.Code, result.Message);
        return null;
      }
    }

    public async Task<ProjectSettingsDataResult> GetProjectSettings(string projectUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetItem<ProjectSettingsDataResult>(projectUid, projectSettingsCacheLife, "PROJECT_SETTINGS_API_URL", customHeaders, $"/{projectUid}");

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

