using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models.Interfaces;
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

    public Task<CompactionProjectSettingsColors> GetProjectSettingsColors(string projectUid, string userId, IHeaderDictionary customHeaders, 
      IServiceExceptionHandler serviceExceptionHandler)
    {
      return GetProjectSettings<CompactionProjectSettingsColors>(projectUid, userId, customHeaders, ProjectSettingsType.Targets, serviceExceptionHandler);
    }

    public Task<CompactionProjectSettings> GetProjectSettingsTargets(string projectUid, string userId, IHeaderDictionary customHeaders,
      IServiceExceptionHandler serviceExceptionHandler)
    {
      return GetProjectSettings<CompactionProjectSettings>(projectUid, userId, customHeaders, ProjectSettingsType.Targets, serviceExceptionHandler);
    }

    private async Task<T> GetProjectSettings<T>(string projectUid, string userId, IHeaderDictionary customHeaders, ProjectSettingsType settingsType,
      IServiceExceptionHandler serviceExceptionHandler) where T : IValidatable, IDefaultSettings, new()
    {
      T ps = default;

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
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(-10, "Unsupported project settings type."));
      }

      var result = await GetMasterDataItemServiceDiscovery<ProjectSettingsResult>(uri, $"{projectUid}{settingsType}", userId, customHeaders);

      if (result.Code == ContractExecutionStatesEnum.ExecutedSuccessfully)
      {
        var jsonSettings = result.Settings;
        if (jsonSettings != null)
        {
          try
          {
            ps = jsonSettings.ToObject<T>();
            if (settingsType == ProjectSettingsType.Colors)
            {
              (ps as CompactionProjectSettingsColors).UpdateCmvDetailsColorsIfRequired();
            }
            ps.Validate(serviceExceptionHandler);

          }
          catch (Exception ex)
          {
            log.LogInformation(
              $"JObject conversion to Project Settings validation failure for projectUid {projectUid}. Error is {ex.Message}");
          }
        }
        else
        {
          log.LogDebug($"No Project Settings for projectUid {projectUid}. Using defaults.");
        }
      }
      else
      {
        log.LogWarning($"Failed to get project settings, using default values: {result.Code}, {result.Message}");
      }

      if (ps == null)
      {
        ps = new T();
        ps.Defaults();
      }
      return ps;
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
