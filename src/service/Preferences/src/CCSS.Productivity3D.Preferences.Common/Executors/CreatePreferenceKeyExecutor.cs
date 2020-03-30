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
  public class CreatePreferenceKeyExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the CreatePreferenceKeyEvent
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var createPrefKeyEvent = CastRequestObjectTo<CreatePreferenceKeyEvent>(item, errorCode: 1);

      // Check name is unique
      if (await preferenceRepo.GetPreferenceKey(prefKeyName: createPrefKeyEvent.PreferenceKeyName) != null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 4, createPrefKeyEvent.PreferenceKeyName);
      }

      // Check UID is unique
      if (await preferenceRepo.GetPreferenceKey(createPrefKeyEvent.PreferenceKeyUID) != null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 5, createPrefKeyEvent.PreferenceKeyUID.ToString());
      }

      var isCreated = 0;
      try
      {    
        isCreated = await preferenceRepo.StoreEvent(createPrefKeyEvent);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 2, "preferenceRepo.storeCreatePreferenceKey", e.Message);
      }

      if (isCreated == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 2);

      var prefKey = await preferenceRepo.GetPreferenceKey(createPrefKeyEvent.PreferenceKeyUID, createPrefKeyEvent.PreferenceKeyName);
      var result = AutoMapperUtility.Automapper.Map<PreferenceKeyV1Result>(prefKey);

      log.LogDebug("CreatePreferenceKey completed successfully");
      return result;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}
