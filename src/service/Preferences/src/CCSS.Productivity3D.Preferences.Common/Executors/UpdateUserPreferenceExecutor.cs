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
  public class UpdateUserPreferenceExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the UpdateUserPreference request
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var updateUserPrefEvent = CastRequestObjectTo<UpdateUserPreferenceEvent>(item, errorCode: 1);

      var isUpdated = 0;
      try
      {
        isUpdated = await preferenceRepo.StoreEvent(updateUserPrefEvent);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 11, "preferenceRepo.storeUpdateUserPreference", e.Message);
      }

      if (isUpdated == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 11);

      var userPref = await preferenceRepo.GetUserPreference(updateUserPrefEvent.UserUID.Value, updateUserPrefEvent.PreferenceKeyName);
      var result = AutoMapperUtility.Automapper.Map<UserPreferenceV1Result>(userPref);

      log.LogDebug("UpdateUserPreference completed successfully");
      return result;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}
