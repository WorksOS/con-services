using ASNode.UserPreferences;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Common.Controllers
{
  public abstract class BaseController : Controller
  {
    private readonly ILogger log;
    private readonly IServiceExceptionHandler serviceExceptionHandler;

    /// <summary>
    /// Gets the custom headers for the request.
    /// </summary>
    /// <value>
    /// The custom headers.
    /// </value>
    protected IDictionary<string, string> customHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// Gets the customer uid form the current context
    /// </summary>
    /// <value>
    /// The customer uid.
    /// </value>
    protected Guid customerUid => GetCustomerUid();


    protected BaseController(ILogger log, IServiceExceptionHandler serviceExceptionHandler)
    {
      this.log = log;
      this.serviceExceptionHandler = serviceExceptionHandler;
    }

    /// <summary>
    /// With the service exception try execute.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action.</param>
    /// <returns></returns>
    protected TResult WithServiceExceptionTryExecute<TResult>(Func<TResult> action) where TResult : ContractExecutionResult
    {
      TResult result = default(TResult);
      try
      {
        result = action.Invoke();
        log.LogTrace($"Executed {action.Method.Name} with result {JsonConvert.SerializeObject(result)}");

      }
      catch (ServiceException se)
      {
        throw new ServiceException(HttpStatusCode.NoContent,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, se.Message));
      }
      catch (Exception ex)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
          ContractExecutionStatesEnum.InternalProcessingError - 2000, ex.Message);
      }
      finally
      {
        log.LogInformation($"Executed {action.Method.Name} with the result {result?.Code}");
      }
      return result;
    }

    /// <summary>
    /// Gets the customer uid form the context.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Incorrect customer uid value.</exception>
    private Guid GetCustomerUid()
    {
      if (User is RaptorPrincipal principal)
        return Guid.Parse(principal.CustomerUid);
      throw new ArgumentException("Incorrect request context principal.");
    }

    /// <summary>
    /// Gets the project identifier.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Incorrect request context principal.</exception>
    protected long GetProjectId(Guid projectUid)
    {
      if (User is RaptorPrincipal principal)
        return (principal.GetProjectId(projectUid));
      throw new ArgumentException("Incorrect request context principal.");
    }

    /// <summary>
    /// Gets the ids of the surveyed surfaces to exclude from Raptor calculations. 
    /// This is the deactivated ones.
    /// </summary>
    /// <param name="fileListProxy">Proxy client to get list of imported files for the project</param>
    /// <param name="projectUid">The UID of the project containing the surveyed surfaces</param>
    /// <returns>The list of file ids for the surveyed surfaces to be excluded</returns>
    protected async Task<List<long>> GetExcludedSurveyedSurfaceIds(IFileListProxy fileListProxy, Guid projectUid)
    {
      var fileList = await fileListProxy.GetFiles(projectUid.ToString(), customHeaders);
      if (fileList == null || fileList.Count == 0)
      {
        return null;
      }

      var results = fileList
        .Where(f => f.ImportedFileType == ImportedFileType.SurveyedSurface && !f.IsActivated)
        .Select(f => f.LegacyFileId).ToList();

      return results;
    }


    /// <summary>
    /// Gets the project settings for the project.
    /// </summary>
    /// <param name="projectSettingsProxy">Proxy client to get project settings for the project</param>
    /// <param name="projectUid">The UID of the project containing the surveyed surfaces</param>
    /// <param name="log">log for logging</param>
    /// <returns>The project settings</returns>
    [Obsolete]
    protected async Task<CompactionProjectSettings> GetProjectSettings(IProjectSettingsProxy projectSettingsProxy,
      Guid projectUid, ILogger log)
    {
      CompactionProjectSettings ps;
      var jsonSettings = await projectSettingsProxy.GetProjectSettings(projectUid.ToString(), customHeaders);
      if (!string.IsNullOrEmpty(jsonSettings))
      {
        try
        {
          ps = JsonConvert.DeserializeObject<CompactionProjectSettings>(jsonSettings);
          ps.Validate();
        }
        catch (Exception ex)
        {
          log.LogInformation(
            $"Project Settings deserialization or validation failure for projectUid {projectUid}. Error is {ex.Message}");
          ps = CompactionProjectSettings.DefaultSettings;
        }
      }
      else
      {
        log.LogDebug($"No Project Settings for projectUid {projectUid}. Using defaults.");
        ps = CompactionProjectSettings.DefaultSettings;
      }
      return ps;
    }


    /// <summary>
    /// Gets the list of contributing machines from the query parameters
    /// </summary>
    /// <param name="assetId">The asset ID</param>
    /// <param name="machineName">The machine name</param>
    /// <param name="isJohnDoe">The john doe flag</param>
    /// <returns>List of machines</returns>
    protected List<MachineDetails> GetMachines(long? assetId, string machineName, bool? isJohnDoe)
    {
      MachineDetails machine = null;
      if (assetId.HasValue || !string.IsNullOrEmpty(machineName) || isJohnDoe.HasValue)
      {
        if (!assetId.HasValue || string.IsNullOrEmpty(machineName) || !isJohnDoe.HasValue)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "If using a machine, asset ID machine name and john doe flag must be provided"));
        }
        machine = MachineDetails.CreateMachineDetails(assetId.Value, machineName, isJohnDoe.Value);
      }
      return machine == null ? null : new List<MachineDetails> { machine };
    }

    /// <summary>
    /// Converts a set user preferences in the format understood by the Raptor for.
    /// It is solely used by production data export WebAPIs.
    /// </summary>
    /// <param name="userPref">The set of user preferences.</param>
    /// <returns>The set of user preferences in Raptor's format</returns>
    protected TASNodeUserPreferences convertUserPreferences(UserPreferenceData userPref)
    {
      TimeZoneInfo projecTimeZone = TimeZoneInfo.FindSystemTimeZoneById(userPref.Timezone);
      double projectTimeZoneOffset = projecTimeZone.GetUtcOffset(DateTime.Now).TotalHours;

      return __Global.Construct_TASNodeUserPreferences(
        userPref.Timezone,
        Preferences.DefaultDateSeparator,
        Preferences.DefaultTimeSeparator,
        userPref.ThousandsSeparator,
        userPref.DecimalSeparator,
        projectTimeZoneOffset,
        Array.IndexOf(LanguageLocales.LanguageLocaleStrings, userPref.Language),
        (int)UnitsTypeEnum.Metric,
        Preferences.DefaultDateTimeFormat,
        Preferences.DefaultNumberFormat,
        Preferences.DefaultTemperatureUnit,
        Preferences.DefaultAssetLabelTypeId);
    }
  }
}