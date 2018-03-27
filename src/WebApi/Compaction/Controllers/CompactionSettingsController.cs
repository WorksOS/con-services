using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for validating 3D project settings
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]

  public class CompactionSettingsController : Controller
  {
    /// <summary>
    /// LoggerFactory for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// For getting project settings for a project
    /// </summary>
    private readonly IProjectSettingsProxy projectSettingsProxy;

    private readonly IResponseCache cache;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="logger">LoggerFactory</param>
    /// <param name="projectSettingsProxy">Project settings proxy</param>
    /// <param name="cache">The memory cache for the controller</param>
    public CompactionSettingsController(ILoggerFactory logger, IProjectSettingsProxy projectSettingsProxy, IResponseCache cache)
    {
      this.log = logger.CreateLogger<CompactionSettingsController>();
      this.projectSettingsProxy = projectSettingsProxy;
      this.cache = cache;
    }

    /// <summary>
    /// Validates 3D project settings.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="projectSettings">Project settings to validate as a JSON object</param>
    /// <param name="settingsType">The project settings' type</param>
    /// <returns>ContractExecutionResult</returns>
    [ProjectUidVerifier]
    [Route("api/v2/validatesettings")]
    [HttpGet]
    public async Task<ContractExecutionResult> ValidateProjectSettings(
      [FromQuery] Guid projectUid,
      [FromQuery] string projectSettings,
      [FromQuery] ProjectSettingsType? settingsType)
    {
      log.LogInformation("ValidateProjectSettings: " + Request.QueryString);

      return ValidateProjectSettingsEx(projectUid.ToString(), projectSettings, settingsType);
    }

    /// <summary>
    /// Validates 3D project settings.
    /// </summary>
    /// <param name="request">Description of the Project Settings request.</param>
    /// <returns>ContractExecutionResult</returns>
    [Route("api/v2/validatesettings")]
    [HttpPost]
    public async Task<ContractExecutionResult> ValidateProjectSettings([FromBody] ProjectSettingsRequest request)
    {
      log.LogDebug($"UpsertProjectSettings: {JsonConvert.SerializeObject(request)}");

      request.Validate();

      return ValidateProjectSettingsEx(request.projectUid, request.Settings, request.ProjectSettingsType);
    }

    private ContractExecutionResult ValidateProjectSettingsEx(string projectUid, string projectSettings, ProjectSettingsType? settingsType)
    {
      if (!string.IsNullOrEmpty(projectSettings))
      {
        if (settingsType == null)
          settingsType = ProjectSettingsType.Targets;

        switch (settingsType)
        {
          case ProjectSettingsType.Targets:
            var compactionSettings = GetProjectSettingsTargets(projectSettings);
            compactionSettings?.Validate();
            break;
          case ProjectSettingsType.Colors:
            var colorSettings = GetProjectSettingsColors(projectSettings);
            colorSettings?.Validate();
            break;
          default:
            throw new ServiceException(HttpStatusCode.InternalServerError,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, $"Unsupported project settings type {settingsType} to validate."));
        }
        //It is assumed that the settings are about to be saved.
        //Clear the cache for these updated settings so we get the updated settings for compaction requests.
        log.LogDebug($"About to clear settings for project {projectUid}");
        ClearProjectSettingsCaches(projectUid + settingsType, Request.Headers.GetCustomHeaders());
        cache.InvalidateReponseCacheForProject(projectUid);
      }
      log.LogInformation("ValidateProjectSettings returned: " + Response.StatusCode);
      return new ContractExecutionResult(ContractExecutionStatesEnum.ExecutedSuccessfully, $"Project settings {settingsType} are valid");
    }

    /// <summary>
    /// Deserializes the project settings targets
    /// </summary>
    /// <param name="projectSettings">JSON representation of the project settings</param>
    /// <returns>The project settings targets instance</returns>
    private CompactionProjectSettings GetProjectSettingsTargets(string projectSettings)
    {
      CompactionProjectSettings ps = null;

      if (!string.IsNullOrEmpty(projectSettings))
      {
        try
        {
          ps = JsonConvert.DeserializeObject<CompactionProjectSettings>(projectSettings);
        }
        catch (Exception ex)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              ex.Message));
        }
      }
      return ps;
    }

    /// <summary>
    /// Deserializes the project settings colors
    /// </summary>
    /// <param name="projectSettings">JSON representation of the project settings</param>
    /// <returns>The project settings colors instance</returns>
    private CompactionProjectSettingsColors GetProjectSettingsColors(string projectSettings)
    {
      CompactionProjectSettingsColors ps = null;

      if (!string.IsNullOrEmpty(projectSettings))
      {
        try
        {
          ps = JsonConvert.DeserializeObject<CompactionProjectSettingsColors>(projectSettings);
        }
        catch (Exception ex)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              ex.Message));
        }
      }
      return ps;
    }

    /// <summary>
    /// Gets the User uid/applicationID from the context.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Incorrect user Id value.</exception>
    private string GetUserId()
    {
      if (User is RaptorPrincipal principal && (principal.Identity is GenericIdentity identity))
      {
        return identity.Name;
      }

      throw new ArgumentException("Incorrect UserId in request context principal.");
    }

    /// <summary>
    /// Clears the project settings cache in the proxy.
    /// </summary>
    /// <param name="projectUid">The project UID that the cached items belong to</param>
    /// <param name="customHeaders">The custom headers of the notification request</param>
    private void ClearProjectSettingsCaches(string projectUid, IDictionary<string, string> customHeaders)
    {
      log.LogInformation("Clearing project settingss cache for project {0}", projectUid);
      //Clear file list cache and reload
      if (!customHeaders.ContainsKey("X-VisionLink-ClearCache"))
        customHeaders.Add("X-VisionLink-ClearCache", "true");

      projectSettingsProxy.ClearCacheItem(projectUid, GetUserId());
    }
  }
}