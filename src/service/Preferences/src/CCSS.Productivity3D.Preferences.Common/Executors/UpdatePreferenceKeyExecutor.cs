using System;
using System.Net;
using System.Threading.Tasks;
using CCSS.Productivity3D.Preferences.Abstractions.ResultsHandling;
using CSS.Productivity3D.Preferences.Common.Utilities;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.VisionLink.Interfaces.Events.Preference;

namespace CCSS.Productivity3D.Preferences.Common.Executors
{
  public class UpdatePreferenceKeyExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the UpdatePreferenceKeyEvent
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var updatePrefKeyEvent = CastRequestObjectTo<UpdatePreferenceKeyEvent>(item, errorCode: 1);

      // Check name is unique
      if (await preferenceRepo.GetPreferenceKey(prefKeyName: updatePrefKeyEvent.PreferenceKeyName) != null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 4, updatePrefKeyEvent.PreferenceKeyName);
      }

      var isUpdated = 0;
      try
      {
        isUpdated = await preferenceRepo.StoreEvent(updatePrefKeyEvent);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 3, "preferenceRepo.storeUpdatePreferenceKey", e.Message);
      }

      if (isUpdated == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 3);

      var prefKey = await preferenceRepo.GetPreferenceKey(updatePrefKeyEvent.PreferenceKeyUID, updatePrefKeyEvent.PreferenceKeyName);
      var result = AutoMapperUtility.Automapper.Map<PreferenceKeyV1Result>(prefKey);

      log.LogDebug("UpdatePreferenceKey completed successfully");
      return result;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}
