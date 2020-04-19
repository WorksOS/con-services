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
  public class CreateUserPreferenceExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the CreateUserPreference request
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var createUserPrefEvent = CastRequestObjectTo<CreateUserPreferenceEvent>(item, errorCode: 1);

      var isCreated = 0;
      try
      {
        isCreated = await preferenceRepo.StoreEvent(createUserPrefEvent);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 10, "preferenceRepo.storeCreateUserPreference", e.Message);
      }

      if (isCreated == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 10);

      var userPref = await preferenceRepo.GetUserPreference(createUserPrefEvent.UserUID, createUserPrefEvent.PreferenceKeyName);
      var result = AutoMapperUtility.Automapper.Map<UserPreferenceV1Result>(userPref);

      log.LogDebug("CreateUserPreference completed successfully");
      return result;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}
