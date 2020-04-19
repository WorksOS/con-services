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
  public class DeletePreferenceKeyExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the DeletePreferenceKeyEvent
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var deletePrefKeyEvent = CastRequestObjectTo<DeletePreferenceKeyEvent>(item, errorCode: 1);

      // Check no user preferences for this key
      if (await preferenceRepo.UserPreferenceExistsForKey(deletePrefKeyEvent.PreferenceKeyUID))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 6, deletePrefKeyEvent.PreferenceKeyUID.ToString());
      }

      var isDeleted = 0;
      try
      {
        isDeleted = await preferenceRepo.StoreEvent(deletePrefKeyEvent);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 7, "preferenceRepo.storeDeletePreferenceKey", e.Message);
      }

      if (isDeleted == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 7); 

      log.LogDebug("DeletePreferenceKey completed successfully");
      return new ContractExecutionResult();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
