using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics.PerformanceData;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Filters.Caching;
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
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// For getting project settings for a project
    /// </summary>
    private readonly IProjectSettingsProxy projectSettingsProxy;

    private readonly IMemoryCacheBuilder<Guid> cacheBuilder;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="projectSettingsProxy">Project settings proxy</param>
    /// <param name="cacheBuilder">The memory cache for the controller</param>
    public CompactionSettingsController(ILoggerFactory logger, IProjectSettingsProxy projectSettingsProxy, IMemoryCacheBuilder<Guid> cacheBuilder)
    {
      this.log = logger.CreateLogger<CompactionSettingsController>();
      this.projectSettingsProxy = projectSettingsProxy;
      this.cacheBuilder = cacheBuilder;
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
        projectSettingsProxy.ClearCacheItem(projectUid.ToString(), GetUserId());
        cacheBuilder.ClearMemoryCache(projectUid);
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

  }
}