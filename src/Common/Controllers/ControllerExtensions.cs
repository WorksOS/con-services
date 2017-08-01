using ASNode.UserPreferences;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Common.Controllers
{
  /// <summary>
  /// Extensions for the Compaction controller.
  /// </summary>
  public static class ControllerExtensions
  {
    /// <summary>
    /// Gets the ids of the surveyed surfaces to exclude from Raptor calculations. 
    /// This is the deactivated ones.
    /// </summary>
    /// <param name="controller">The controller which received the request</param>
    /// <param name="fileListProxy">Proxy client to get list of imported files for the project</param>
    /// <param name="projectUid">The UID of the project containing the surveyed surfaces</param>
    /// <param name="customHeaders">Http request custom headers</param>
    /// <returns>The list of file ids for the surveyed surfaces to be excluded</returns>
    public static async Task<List<long>> GetExcludedSurveyedSurfaceIds(this Controller controller, IFileListProxy fileListProxy, Guid projectUid, IDictionary<string, string> customHeaders)
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
    /// <param name="controller">The controller which received the request</param>
    /// <param name="projectSettingsProxy">Proxy client to get project settings for the project</param>
    /// <param name="projectUid">The UID of the project containing the surveyed surfaces</param>
    /// <param name="customHeaders">Http request custom headers</param>
    /// <param name="log">log for logging</param>
    /// <returns>The project settings</returns>
    public static async Task<CompactionProjectSettings> GetProjectSettings(this Controller controller, IProjectSettingsProxy projectSettingsProxy, Guid projectUid, IDictionary<string, string> customHeaders, ILogger log)
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
    /// <param name="controller">The controller which received the reques</param>
    /// <param name="assetId">The asset ID</param>
    /// <param name="machineName">The machine name</param>
    /// <param name="isJohnDoe">The john doe flag</param>
    /// <returns>List of machines</returns>
    public static List<MachineDetails> GetMachines(this Controller controller, long? assetId, string machineName, bool? isJohnDoe)
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
    /// Replaces a service exception's BadRequest status code with the NoContent one.
    /// </summary>
    /// <param name="controller">The controller which received the request.</param>
    /// <param name="serviceException">The ServiceException instance.</param>
    public static void ProcessStatusCode(this Controller controller, ServiceException serviceException)
    {
      if (serviceException.Code == HttpStatusCode.BadRequest &&
          serviceException.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
      {
        serviceException.Code = HttpStatusCode.NoContent;
      }
    }

    /// <summary>
    /// Converts a set user preferences in the format understood by the Raptor for.
    /// It is solely used by production data export WebAPIs.
    /// </summary>
    /// <param name="controller">The controller which received the request.</param>
    /// <param name="userPref">The set of user preferences.</param>
    /// <returns>The set of user preferences in Raptor's format</returns>
    public static TASNodeUserPreferences convertUserPreferences(this Controller controller, UserPreferenceData userPref)
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